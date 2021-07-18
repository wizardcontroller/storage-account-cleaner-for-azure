import { AvailableCommand, WorkflowOperation, WorkflowOperationCommand } from "@wizardcontroller/sac-appliance-lib";

export class WorkflowOperationCommandImpl {
  candidateCommand?: AvailableCommand;
  timeStamp?: string;
  readonly displayMessage?: string | null;
  commandCode?: WorkflowOperation;
}
