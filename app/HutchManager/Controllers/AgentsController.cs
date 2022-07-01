using HutchManager.Models;
using HutchManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HutchManager.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgentsController : ControllerBase
{
  private readonly DataSourceService _dataSources;
  public AgentsController(DataSourceService dataSources)
  {
    _dataSources = dataSources;
  }

  [HttpPost]
  public async Task<List<DataSource>> CheckIn(List<DataSource> dataSource)
  {
    List<DataSource> dataSourceList = new List<DataSource>();
    for (int i = 0; i < dataSource.Count; i++)
    {
      DataSource d = await _dataSources.CreateorUpdate(dataSource[i]);
      dataSourceList.Add(d);
    }
    return dataSourceList;
  }

}