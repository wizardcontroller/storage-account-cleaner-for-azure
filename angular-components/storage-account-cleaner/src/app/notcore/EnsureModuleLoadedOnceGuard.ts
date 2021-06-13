
// as per Pluralsight Angular Architecture and Best Practicves
export class EnsureModuleLoadedOnceGuard {

  constructor(targetModule: any){
    if(targetModule){
      throw new Error(`${targetModule.constructor.name} has already been loaded.
      Import this module in the AppModule only.`);
    }
  }
}
