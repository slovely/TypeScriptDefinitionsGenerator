module Api {
  export interface IDictionary<T> {
     [key: string]: T;
  }


  export class Person {
    public static getPerson(ajaxOptions: IExtendedAjaxSettings = null): JQueryPromise<SampleWebsite.Models.Person> {
      return ServiceCaller.post("api/Person/getPerson", null, ajaxOptions);
    }

  }
}