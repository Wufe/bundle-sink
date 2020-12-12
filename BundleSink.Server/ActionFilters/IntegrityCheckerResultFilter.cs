using System;
using System.Linq;
using BundleSink.Models;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BundleSink.ActionFilters
{
    public class IntegrityCheckerResultFilterAttribute : ResultFilterAttribute {
        private readonly EntriesViewData _entriesViewData;

        public IntegrityCheckerResultFilterAttribute(
            EntriesViewData entriesViewData
        )
        {
            _entriesViewData = entriesViewData;
        }

        public override void OnResultExecuted(ResultExecutedContext context)
        {
            var unprocessed = _entriesViewData.RequestedEntries
                .Where(entry => !entry.Processed);
            if (unprocessed.Any())
            {
                throw new Exception(
                    $"Following entries have not been processed.\n" +
                    $"Pay attention to execution order of tag helpers.\n\n" +
                    $"More info here https://github.com/Wufe/bundle-sink#pay-attention-to-execution-order .\n\n" +
                    $"Unprocessed entries: " + string.Join(',', unprocessed.Select(entry => entry.Name ?? entry.GetIdentifier()))
                );
            }
            base.OnResultExecuted(context);
        }
    }
}