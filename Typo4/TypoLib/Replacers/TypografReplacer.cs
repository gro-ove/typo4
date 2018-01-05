using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Services;
using System.Web.Services.Description;
using System.Web.Services.Protocols;
using JetBrains.Annotations;

namespace TypoLib.Replacers {
    [DebuggerStepThrough, DesignerCategory("code"), WebServiceBinding(Name = "TypografSoap", Namespace = "http://typograf.artlebedev.ru/webservices/")]
    public class TypografReplacer : SoapHttpClientProtocol, IReplacer {
        private enum EntityType {
            Symbols = 0,
            Html = 1,
            Xml = 2,
            No = 3,
            Mixed = 4
        }

        public TypografReplacer() {
            Url = "http://typograf.artlebedev.ru/webservices/typograf.asmx";
        }

        [SoapDocumentMethod("http://typograf.artlebedev.ru/webservices/ProcessText", RequestNamespace = "http://typograf.artlebedev.ru/webservices/",
                 ResponseNamespace = "http://typograf.artlebedev.ru/webservices/", Use = SoapBindingUse.Literal, ParameterStyle = SoapParameterStyle.Wrapped)]
        public string ProcessText(string text, int entityType, bool useBr, bool useP, int maxNobr) {
            var results = Invoke("ProcessText", new object[] { text, entityType, useBr, useP, maxNobr });
            return (string)results[0];
        }

        public IAsyncResult BeginProcessText(string text, int entityType, bool useBr, bool useP, int maxNobr, AsyncCallback callback, object asyncState) {
            return BeginInvoke("ProcessText", new object[] { text, entityType, useBr, useP, maxNobr}, callback, asyncState);
        }

        public string EndProcessText(IAsyncResult asyncResult) {
            return (string)EndInvoke(asyncResult)[0];
        }

        private Task<string> ProcessTextAsync([NotNull] string text, int entityType, bool useBr, bool useP, int maxNobr, CancellationToken cancellation) {
            return Task.Run(() => ProcessText(text, entityType, useBr, useP, maxNobr));
        }

        public void Initialize(string dataDirectory) {}

        public Task<string> ReplaceAsync(string originalText, CancellationToken cancellation) {
            return originalText == null ? Task.FromResult<string>(null) : ProcessTextAsync(originalText, (int)EntityType.No, false, false, 3, cancellation);
        }
    }
}
