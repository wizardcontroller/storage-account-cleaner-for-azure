export class ToastMessage {
  id!: string;
  severity!: string;
  summary!: string;
  detail!: string;
  sticky: boolean = true;
  closeable: boolean = true;
  life: number = 1000 * 30; // 30 seconds
  key!: string;
}
