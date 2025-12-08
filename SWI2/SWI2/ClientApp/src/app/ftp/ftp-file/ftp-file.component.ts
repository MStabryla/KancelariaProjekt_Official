import { HttpErrorResponse, HttpEvent, HttpEventType } from '@angular/common/http';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { AlertData } from '../../alert/alert.component';
import { ApiService } from '../../shared/services/api.service';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';
import { FileModel } from '../file.model';
import { FtpFileViewComponent } from '../ftp-file-view/ftp-file-view.component';
import { FtpUploadComponent } from '../ftp-upload/ftp-upload.component';

@Component({
    selector: 'swi-ftp-file',
    templateUrl: './ftp-file.component.html',
    styleUrls: ['./ftp-file.component.css']
})
/** ftp-file component*/
export class FtpFileComponent {
  public renaming = false;
  public renamingValue: string;

  public creatingFolder = false;
  public creatingFolderValue: string;

  @Input() file: FileModel;
  @Input() public fileTableRef: FileModel;
  @Input() root: boolean;
  @Input() folderState: number; // 0 - READ, 1 - CREATE, 2 - ALL

  @Output() clickedEvent: EventEmitter<FileModel> = new EventEmitter();
  @Output() refreshEvent: EventEmitter<FileModel> = new EventEmitter();

  /** ftp-file ctor */
  constructor(private _api: ApiService, private errorD: ErrorDialogService, private dialog: MatDialog) {
    
  }

  ngOnInit() {
    this.renamingValue = this.file.name.split('.')[0];
  }

  clicked() {
    this.clickedEvent.emit(this.file.type === "dir" ? null : this.file);
  }

  createDirectory() {
    if (this.file.type !== "dir")
      return;
    this.creatingFolder = true;
  }
  onCreateDirectorKeydown(event: KeyboardEvent) {
    if (event.key === "Enter" && this.creatingFolderValue === "")
      this.creatingFolder = false;
    else if (event.key === "Enter")
      this.createDirectoryConfirm();
  }
  createDirectoryConfirm() {
    if (this.file.type !== "dir")
      return;
    const url = "/api/filepanel/folder?path=" + btoa(this.file.path) + "&folderName=" + this.creatingFolderValue;

    this._api.post(url, null).toPromise().then((x: any) => {
      this.creatingFolder = false;
      this.creatingFolderValue = "";
      this.clickedEvent.emit(new FileModel(x));
      //this.file.children.push();
      //this.file.children = this.file.children.sort((a) => a.type === 'dir' ? -1 : 1);
    }).catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
      this.creatingFolder = false;
      this.creatingFolderValue = "";
    })
  }

  rename() {
    this.renaming = !this.renaming;
  }
  onRenameKeydown(event: KeyboardEvent) {
    if (event.key === "Enter")
      this.renameConfirm();
  }
  renameConfirm() {
    const newName = this.renamingValue + (this.file.type !== "dir" ? "." + this.file.name.split('.')[1] : "");
    const url = "/api/filepanel/?path=" + btoa(this.file.path) + "&newName=" + newName;
    this._api.put(url, null).toPromise().then((x: any) => {
      //this.errorD.showAlert(new AlertData("Renamed", "File " + this.file.name + " succesfully renamed to " + x.data.name));
      this.renaming = false;
      this.file.name = x.data.name;
      this.file.path = x.data.fullName;
    }).catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
      this.renaming = false;
    })
  }

  delete() {
    let url = "/api/filepanel/?path=" + btoa(this.file.path);
    if (this.file.type === "dir")
      url = "/api/filepanel/folder?path=" + btoa(this.file.path);
    this._api.delete(url).toPromise().then(() => {
      //this.errorD.showAlert(new AlertData("Renamed", "File " + this.file.name + " succesfully renamed to " + x.data.name));
      this.refreshEvent.emit(this.file);
    }).catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    })
  }

  downloadUrl() { return "/api/filepanel/download?path=" + btoa(this.file.path) + "&access_token=" + localStorage.getItem("jwt"); }

  upload() {
    this.dialog.open(FtpUploadComponent, {
      maxHeight: '96vh',
      height: 'auto',
      width: '90vw',
      data: this.file
    }).afterClosed().subscribe(() => {
      this.clickedEvent.emit(this.file);
    }, (x: HttpErrorResponse) => {
        this.errorD.showDialog(x);
    });
  }

  
}
