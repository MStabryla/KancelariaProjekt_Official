import { HttpEventType, HttpResponse } from '@angular/common/http';
import { Component, HostListener, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { finalize } from 'rxjs/operators';
import { ApiService } from '../../shared/services/api.service';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';
import { FileModel } from '../file.model';

@Component({
    selector: 'swi-ftp-upload',
    templateUrl: './ftp-upload.component.html',
    styleUrls: ['./ftp-upload.component.css']
})
/** ftp-upload component*/
export class FtpUploadComponent {

  public fileDragged = false;
  public sending = false;
  public fileList: File[];

  constructor(private _api: ApiService,
    @Inject(MAT_DIALOG_DATA) public data: FileModel,
    private errorD: ErrorDialogService,
    private dialogRef: MatDialogRef<File>) {
    this.fileList = [];
  }

  onDragOver(event) {
    this.fileDragged = true;
  }

  onDragLeave(event) {
    this.fileDragged = false;
  }

  onDrop(event) {

    this.fileDragged = false;

    for (const item of event.dataTransfer.files) {
      this.fileList.push(item);
    }
  }
  onChange(event) {
    this.fileList = event.target.files;
  }
  progress: number[];
  uploadFiles() {
    this.sending = true;
    this.progress = [];
    for (let i = 0; i < this.fileList.length; i++) {
      const formData = new FormData();
      const item = this.fileList[i];
      formData.append("file", item);
      this.progress.push(0);
      const url = "/api/filepanel/?path=" + btoa(this.data.path);
      this._api.post(url, formData, { responseType: "blob", reportProgress: true, observe: "events" }, { reportProgress: true }).pipe(
        finalize(() => {
          this.sending = false;
          this.dialogRef.close({ op: "upload", data: event })
        })
      ).subscribe(
        (event: any) => {
          if (event.type === HttpEventType.UploadProgress) {
            this.progress[i] = Math.round(
              (100 * event.loaded) / event.total
            );
          } else if (event instanceof HttpResponse) {
            this.dialogRef.close({ op: "upload", data: event })
          }
        },
        (error) => {
          this.errorD.showDialog(error);
        }
      );
    }
  } 

  sizeView(data: File): string {
    if (data.size >= Math.pow(10, 6)) {
      let size = data.size;
      return (size /= Math.pow(10, 6)) + " MB";
    }
    else if (data.size >= Math.pow(10, 3)) {
      let size = data.size;
      return (size /= Math.pow(10, 3)) + " KB";
    }
    else
      return data.size + " B";
  }
}
