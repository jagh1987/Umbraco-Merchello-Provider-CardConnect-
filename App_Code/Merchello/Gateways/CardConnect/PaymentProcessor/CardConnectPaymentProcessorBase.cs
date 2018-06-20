using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

using Merchello.Core.Models;
using Merchello.Core.Gateways.Payment;
using Merchello.CardConnect.Models;

using Umbraco.Core;

using IPayment = Merchello.Core.Models.IPayment;
using IPaymentResult = Merchello.Core.Gateways.Payment.IPaymentResult;
using Constants = Merchello.Plugin.Payments.CardConnect.Constants;
using System.Net.Http;
using Merchello.CardConnect.Services;

namespace Merchello.CardConnect.PaymentProcessor
{
    public class CardConnectPaymentProcessorBase
    {
        public readonly ICardConnectService CardConnectService;
        public readonly string MerchId;

        public CardConnectPaymentProcessorBase(ICardConnectService cardConnectService)
        {
            CardConnectService = cardConnectService;
            MerchId = cardConnectService.MerchId;
        }
        
        /// <summary>
         /// Get the absolute base URL for this website
         /// </summary>
         /// <returns></returns>
        protected static string GetWebsiteUrl()
        {
            var url = HttpContext.Current.Request.Url;
            var baseUrl = String.Format("{0}://{1}{2}", url.Scheme, url.Host, url.IsDefaultPort ? "" : ":" + url.Port);
            return baseUrl;
        }
        public IPaymentResult AuthorizePayment(IInvoice invoice, IPayment payment)
        {
            try
            {
                payment.ExtendedData.SetValue(Constants.ExtendedDataKeys.Authorization, "true");
                payment.Authorized = true;
            }
            catch (Exception ex)
            {
                return new PaymentResult(Attempt<IPayment>.Fail(payment, ex), invoice, false);
            }

            return new PaymentResult(Attempt<IPayment>.Succeed(payment), invoice, true);
        }

        public IPaymentResult CapturePayment(IInvoice invoice, IPayment payment, decimal amount, bool isPartialPayment)
        {
            try
            {
                payment.ExtendedData.SetValue(Constants.ExtendedDataKeys.Capture, "true");
                payment.Collected = true;
            }
            catch (Exception ex)
            {
                return new PaymentResult(Attempt<IPayment>.Fail(payment, ex), invoice, false);
            }

            return new PaymentResult(Attempt<IPayment>.Succeed(payment), invoice, true);
        }

        protected Exception CreateErrorResult(HttpContent errors)
        {
            //var errorText = errors.Count == 0 ? "Unknown error" : ("- " + string.Join("\n- ", errors.Select(item => item.LongMessage)));
            return new Exception(errors.ToString());
        }

        protected Exception CreateErrorResult(NameValueCollection errors)
        {
            return new Exception(errors.Cast<string>().Select(e => errors[e]).ToString());
        }
    }
}