import { MatButtonToggleChange, MatButtonToggleGroup } from "@angular/material/button-toggle";
import { BehaviorSubject } from "rxjs";
import { Observable, ReplaySubject, Subject } from "rxjs";

// tslint:disable-next-line:interface-name
export interface IToggleToolChooser {
  toolSelector: MatButtonToggleGroup;
  selectedView: string;
  toolSelectionSource : BehaviorSubject<string>;
  toolSelectionChanges$ : Observable<string>;
  toolChanged(e: MatButtonToggleChange): void;
}
