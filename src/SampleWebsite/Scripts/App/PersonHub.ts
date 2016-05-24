class PersonHub {

    private static initialized: boolean = false;

    constructor() {


        if (!PersonHub.initialized) {
            const hub = $.connection.personHub;

            // Implement all the client-side callbacks defined on IPersonHubClient (in hubs.d.ts)
            hub.client.personCreated = p => {
                console.log("A person has been created [Name = " + p.Name + ", Gender = " + p.Gender + "]");
            };

            PersonHub.initialized = true;
        }

    }

}
