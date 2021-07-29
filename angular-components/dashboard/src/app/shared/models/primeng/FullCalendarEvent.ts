export class FullCalendarEvent {
  id!: string;
  title!: string;
  start!: Date;
  end!: Date;
  rendering!: string;
  color = "rgb(3, 131, 135)";
  textColor= "rgb(3, 131, 135)";
  backgroundColor ="rgba(0, 131, 135, 0.35)";
  allDay = false;
}
