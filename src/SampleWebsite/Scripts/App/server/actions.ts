module Api {
  export interface IDictionary<T> {
     [key: string]: T;
  }


  export class Person {
    public static actionMethodWithBodyAndQueryString(body: string, queryId: number, ajaxOptions: IExtendedAjaxSettings = null): JQueryPromise<number> {
      return ServiceCaller.get("api/Person/actionMethodWithBodyAndQueryString?queryId=" + queryId + "", body, ajaxOptions);
    }

    public static create(person: any, ajaxOptions: IExtendedAjaxSettings = null): JQueryPromise<SampleWebsite.Models.Person> {
      return ServiceCaller.get("api/Person/create", person, ajaxOptions);
    }

    public static getPerson(ajaxOptions: IExtendedAjaxSettings = null): JQueryPromise<SampleWebsite.Models.Person> {
      return ServiceCaller.post("api/Person/getPerson", null, ajaxOptions);
    }

    public static getPersonCustom(personid: number, ajaxOptions: IExtendedAjaxSettings = null): JQueryPromise<SampleWebsite.Models.Person> {
      return ServiceCaller.get("api/Person/getPersonCustom?personid=" + personid + "", null, ajaxOptions);
    }

  }
}