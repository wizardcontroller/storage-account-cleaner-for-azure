import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'datesToTimeLineEventsPipe'
})
export class DatesToTimeLineEventsPipePipe implements PipeTransform {

  transform(value: unknown, ...args: unknown[]): unknown {
    return null;
  }

}
