using System.Text.Json.Nodes;

namespace HutchAgent.Config;

public class LicenseOptions
{
  /// <summary>
  /// The @id of the license entity to add to the outputs.
  /// </summary>
  public string Uri { get; set; } = string.Empty;

  /// <summary>
  /// This attribute contains any other properties for the license entity.
  /// </summary>
  public JsonObject? Properties { get; set; } = null;
}
