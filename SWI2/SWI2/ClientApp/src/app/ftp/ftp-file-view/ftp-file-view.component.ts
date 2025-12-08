import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { FileModel } from '../file.model';

@Component({
    selector: 'swi-ftp-file-view',
    templateUrl: './ftp-file-view.component.html',
    styleUrls: ['./ftp-file-view.component.css']
})
/** ftp-file-view component*/
export class FtpFileViewComponent {
  sizeUnit = "B";
  /** ftp-file-view ctor */
  constructor(@Inject(MAT_DIALOG_DATA) public data: FileModel) {
      
  }

  ngOnInit() {
    if (this.data.size >= Math.pow(10, 6)) {
      this.data.size /= Math.pow(10, 6);
      this.sizeUnit = "MB"; 
    }
    else if (this.data.size >= Math.pow(10, 3)) {
      this.data.size /= Math.pow(10, 3);
      this.sizeUnit = "KB"; 
    }
  }
}
