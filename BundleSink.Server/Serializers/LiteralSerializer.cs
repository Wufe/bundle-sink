using System.IO;
using System.Linq;
using BundleSink.Models;
using BundleSink.Models.Entry;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;

namespace BundleSink.Server.Serializers
{
    public partial class Serializer
    {
        private string Serialize(IRequestedLiteralEntryModel literalEntry, string dependencyOf = null)
        {
            var finalOutput = "";
            if (_settings.PrintComments)
            {
                finalOutput += string.Format(
                    "\n<!-- <literal-entry{0}> -->\n",
                    SerializeEntryAttributes(literalEntry, dependencyOf)
                );
            }
            finalOutput += literalEntry.Content + "\n";
            if (_settings.PrintComments)
            {
                finalOutput += $"<!-- </literal-entry> -->";
            }
            return finalOutput;
        }
    }
}