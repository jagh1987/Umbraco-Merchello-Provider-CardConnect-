namespace Merchello.CardConnect.Models
{
    using Merchello.FastTrack.Models.Payment;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Globalization;
    using System.Linq;
    using System.Web.Mvc;

    public class CardConnectPaymentModel : FastTrackPaymentModel
    {
        /// <summary>
        /// Gets or sets the Card Holder Name
        /// </summary>
        [Required(ErrorMessage = "Please enter the cardholder name")]
        [DisplayName(@"Holder Name")]
        public string CardHolder { get; set; }

        /// <summary>
        /// Gets or sets the Card Number
        /// </summary>
        [Required(ErrorMessage = "Please enter the credit card numer")]
        [DisplayName(@"Card Number")]
        public string CardNumber { get; set; }

        /// <summary>
        /// Gets or sets the Expiration Date
        /// </summary>
        [Required(ErrorMessage = "Month expiration")]
        [DisplayName(@"Expiration Month")]
        public string ExpiresMonth { get; set; }

        /// <summary>
        /// Gets or sets the Expiration Date
        /// </summary>
        [Required(ErrorMessage = "Year expiration")]
        [DisplayName(@"Expiration Year")]
        public string ExpiresYear
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Card Verification Value
        /// </summary>
        [Required(ErrorMessage = "Enter security code")]
        [DisplayName(@"Cvv Security Code")]
        public string Cvv { get; set; }

        public IEnumerable<SelectListItem> Months
        {
            get
            {
                IEnumerable<SelectListItem> data = DateTimeFormatInfo
                       .InvariantInfo
                       .MonthNames
                       .Select((monthName, index) => new SelectListItem
                       {
                           Value = ((index + 1)<10)? "0"+(index + 1).ToString(): (index + 1).ToString(),
                           Text = monthName
                       });
                return data.Where(x=>x.Text!=string.Empty);
            }
        }

        public IEnumerable<SelectListItem> Years
        {
            get
            {
                return new SelectList(Enumerable.Range(DateTime.Now.Year - 2000, 6).ToList());
            }
        }

        public string ErrorMessage { get; set; }
    }
}