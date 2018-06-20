using Merchello.Core.Models;
using Merchello.CardConnect.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Merchello.Plugin.Payments.CardConnect
{
    public static class MappingExtensions
    {
        /// <summary>
        /// Saves the processor settings to an extended data collection
        /// </summary>
        /// <param name="extendedData">The <see cref="ExtendedDataCollection"/></param>
        /// <param name="processorSettings">The <see cref="CardConnectProcessorSettings"/> to be serialized and saved</param>
        public static void SaveProcessorSettings(this ExtendedDataCollection extendedData, CardConnectProcessorSettings processorSettings)
        {
            var settingsJson = JsonConvert.SerializeObject(processorSettings);

            extendedData.SetValue(Constants.ExtendedDataKeys.ProcessorSettings, settingsJson);
        }

        /// <summary>
        /// Get the processor settings from the extended data collection
        /// </summary>
        /// <param name="extendedData">The <see cref="ExtendedDataCollection"/></param>
        /// <returns>The deserialized <see cref="CardConnectProcessorSettings"/></returns>
        public static CardConnectProcessorSettings GetProcessorSettings(this ExtendedDataCollection extendedData)
        {
            if (!extendedData.ContainsKey(Constants.ExtendedDataKeys.ProcessorSettings)) return new CardConnectProcessorSettings();

            CardConnectProcessorSettings settings = JsonConvert.DeserializeObject<CardConnectProcessorSettings>(extendedData.GetValue(Constants.ExtendedDataKeys.ProcessorSettings));

            return settings;
        }
    }
}