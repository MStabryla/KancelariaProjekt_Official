import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
const content = [
  { name: 'RANGE_OF_SERVICES', icon: { type: 'svg', name: '4.svg' } },
  { name: 'DISTRIBUTION', icon: { type: 'svg', name: '5.svg' } },
  { name: 'UPDATES', icon: { type: 'svg', name: '6.svg' } },
  { name: 'OUR_CLIENTS', icon: { type: 'svg', name: '2.svg' } },
  { name: 'SERVICES', icon: { type: 'svg', name: '4.svg' } },
  { name: 'NEWS', icon: { type: 'svg', name: '6.svg' } },
  { name: 'STAFF', icon: { type: 'icon', name: 'support_agent' } },
  { name: 'BRANCHES', icon: { type: 'icon', name: 'apartment' } },
  { name: 'SOFTWARE', icon: { type: 'icon', name: 'api' } },

]
@Component({
  selector: 'main-subpage',
  templateUrl: './main-subpage.component.html',
  styleUrls: ['./main-subpage.component.css']
})
export class MainSubpageComponent implements OnInit {
  subPageNumber: any;
  translationElements: any;

  constructor(private _Activatedroute: ActivatedRoute) {

  }

  ngOnInit(): void {
    this._Activatedroute.params.subscribe(params => {
      this.subPageNumber = content[this._Activatedroute.snapshot.paramMap.get("id")];
      this.translationElements = "SUBPAGE." + this.subPageNumber.name + ".ELEMENTS";
    });
  }

}
