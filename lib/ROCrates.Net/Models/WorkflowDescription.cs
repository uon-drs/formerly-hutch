using System.Text.Json;
using System.Text.Json.Nodes;
using ROCrates.Converters;

namespace ROCrates.Models;

/// <summary>
/// Abstract CWL description of the main workflow.
/// </summary>
public class WorkflowDescription : ComputationalWorkflow
{
  public WorkflowDescription(ROCrate crate, string? identifier = null, JsonObject? properties = null,
    string? source = null, string? destPath = null, bool fetchRemote = false, bool validateUrl = false) : base(crate,
    identifier, properties, source, destPath, fetchRemote, validateUrl)
  {
    Types = new[] { "File", "SoftwareSourceCode", "HowTo" };
    SetProperty("@type", Types);
  }

  /// <summary>
  /// Convert <see cref="WorkflowDescription"/> to JSON string.
  /// </summary>
  /// <returns>The <see cref="WorkflowDescription"/> as a JSON string.</returns>
  public override string Serialize()
  {
    var options = new JsonSerializerOptions
    {
      WriteIndented = true,
      Converters = { new WorkflowDescriptionConverter() }
    };
    var serialised = JsonSerializer.Serialize(this, options);
    return serialised;
  }
}