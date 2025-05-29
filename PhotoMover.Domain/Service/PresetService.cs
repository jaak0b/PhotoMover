using System.Collections.ObjectModel;
using Domain.Model;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Domain.Service
{
  public interface IPresetService
  {
  }

  public class PresetService(Database db) : IPresetService
  {
  }
}