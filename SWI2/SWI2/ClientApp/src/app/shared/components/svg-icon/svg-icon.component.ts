import { Component, Input, OnInit } from '@angular/core';
import { MatIconRegistry } from '@angular/material/icon';
import { DomSanitizer } from '@angular/platform-browser';

@Component({
  selector: 'app-svg-icon',
  templateUrl: './svg-icon.component.html',
  styleUrls: ['./svg-icon.component.css']
})
export class SvgIconComponent implements OnInit {

  @Input() name: string;
  @Input() heigth: string;
  @Input() width: string;
  constructor(private matIconRegistry: MatIconRegistry,
    private domSanitizer: DomSanitizer) {

  }
  ngOnInit(): void {
    this.matIconRegistry.addSvgIcon(
      this.name,
      this.domSanitizer.bypassSecurityTrustResourceUrl("/assets/images/" + this.name));
  }

}
