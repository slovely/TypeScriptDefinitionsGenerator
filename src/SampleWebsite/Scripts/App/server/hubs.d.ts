interface ISignalRPromise<T> {
    done(cb: (result: T) => any): ISignalRPromise<T>;
    error(cb: (error: any) => any): ISignalRPromise<T>;
}

interface IPersonHubClient {
    personCreated(person: SampleWebsite.Models.Person): void;
}

interface IPersonHub {
    createPerson(person: SampleWebsite.Models.Person): void;
}

interface IPersonHubProxy {
    server: IPersonHub;
    client: IPersonHubClient;
}

interface ITimeHubClient {
    /* Client interface not generated as hub doesn't derive from Hub<T> */
}

interface ITimeHub {
    currentServerTime(): ISignalRPromise<string>;
}

interface ITimeHubProxy {
    server: ITimeHub;
    client: ITimeHubClient;
}

interface SignalR {
    personHub: IPersonHubProxy;
    timeHub: ITimeHubProxy;
}

