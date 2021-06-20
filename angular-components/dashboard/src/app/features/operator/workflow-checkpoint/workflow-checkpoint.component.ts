import { PageModelService } from './../../../shared/page-model.service';
import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-workflow-checkpoint',
  templateUrl: './workflow-checkpoint.component.html',
  styleUrls: ['./workflow-checkpoint.component.css']
})
export class WorkflowCheckpointComponent implements OnInit {

  constructor(private pageModelService: PageModelService) { }

  ngOnInit(): void {
    console.log("getting workflow state");

  }

}
