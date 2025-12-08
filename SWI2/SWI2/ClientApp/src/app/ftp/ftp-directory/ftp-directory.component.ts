import { NestedTreeControl } from '@angular/cdk/tree';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, EventEmitter, Output } from '@angular/core';
import { MatTreeNestedDataSource } from '@angular/material/tree';
import { ApiService } from '../../shared/services/api.service';
import { ErrorDialogService } from '../../shared/services/error-dialog-service';
import { FileModel } from '../file.model';
import { Input } from '@angular/core';
import { FtpFileViewComponent } from '../ftp-file-view/ftp-file-view.component';
import { MatDialog } from '@angular/material/dialog';

@Component({
  selector: 'swi-ftp-directory',
  templateUrl: './ftp-directory.component.html',
  styleUrls: ['./ftp-directory.component.css']
})
/** ftp-directory component*/
export class FtpDirectoryComponent {

  @Input() public dirModel: FileModel;
  @Input() public root: boolean;
  @Input() public parentRoot: boolean;
  @Input() public folderState: number; // 0 - READ, 1 - CREATE, 2 - ALL

  @Output() public parentRefresh: EventEmitter<true> = new EventEmitter();

  childFiles: FileModel[];
  isExpanded = false;

  /** ftp-directory ctor */
  constructor(private _api: ApiService, private errorD: ErrorDialogService, public dialog: MatDialog) { 
  }

  ngOnInit() {
    if (this.root)
      this.getChildren(null);
  }

  getChildren(expand = false) {
    this.isExpanded = expand === null ? !this.isExpanded : expand;
    this.dirModel.dirOpen = expand === null ? !this.dirModel.dirOpen : expand;
    const path = this.dirModel && this.dirModel.path !== "home" ? "/api/filepanel?path=" + btoa(this.dirModel.path) : "/api/filepanel/home";
    this._api.get(path).toPromise().then((data: any) => {
      this.dirModel.children = data.childFiles.map(x => {
        return new FileModel(x);
      }).sort((a) => a.type === 'dir' ? -1 : 1);
    }).catch((x: HttpErrorResponse) => {
      this.errorD.showDialog(x);
    })
  }

  public getParentChildren() {
    this.parentRefresh.emit(true);
  }

  getDetails(data: FileModel) {
    if (data.type === "dir")
      return;
    this.dialog.open(FtpFileViewComponent, {
      maxHeight: '96vh',
      height: 'auto',
      width: '90vw',
      data: data
    });
  }

}
