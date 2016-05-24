module Api {

    declare var rootPath: string;

    export interface IExtendedAjaxSettings extends JQueryAjaxSettings {
        /**
         * Allows the default error handling to be suppressed.
         */
        preventDefaultErrorHandler?: boolean;
    }

    export class ServiceCaller {

        private static ajaxDefaults: JQueryAjaxSettings = {
            cache: false,
            dataType: "json",
            timeout: 120000,
            crossDomain: false
        }

        public static post(url: string, data: any = null, ajaxOptions: Api.IExtendedAjaxSettings = null): JQueryPromise<any> {
            //Apply custom ajaxsettings
            var settings = $.extend({}, this.ajaxDefaults, ajaxOptions);
            settings.type = 'POST';
            // Allow data to be overridden by passing it in via ajaxOptions parameter.
            if (!settings.data) settings.data = data;

            // Workaround for arrays: http://aspnetwebstack.codeplex.com/workitem/177
            if (settings.data instanceof Array) {
                settings.data = { '': settings.data };
            }
            return $.ajax(rootPath + url, settings).fail((jqXhr: JQueryXHR) => {
                this.defaultErrorHandler(settings, jqXhr);
            });
        }

        public static get(url: string, data = null, ajaxOptions: Api.IExtendedAjaxSettings = null) {
            //Apply custom ajaxsettings
            var settings = $.extend({}, this.ajaxDefaults, ajaxOptions);
            settings.type = 'GET';

            return $.ajax(rootPath + url, settings).fail((jqXhr: JQueryXHR) => {
                this.defaultErrorHandler(settings, jqXhr);
            });
        }

        private static defaultErrorHandler(settings: IExtendedAjaxSettings, jqXhr: JQueryXHR): JQueryPromise<any> {
            if (settings.preventDefaultErrorHandler) return;

            // Use durandal showMessage function if it's available, otherwise fallback to alert.
            var alertMethod = (<any>window).app ? (<any>window).app.showMessage : alert;
            var ex = JSON.parse(jqXhr.responseText);
            if (ex.ExceptionMessage != null) {
                alertMethod(ex.ExceptionMessage);
            } else if (ex.Message != null) {
                alertMethod(ex.Message);
            } else {
                alertMethod(jqXhr.responseText);
            }
        }
    }
}