using System;

namespace MeasurementApp.Contracts.Services
{
    public interface IPageService
    {
        Type GetPageType(string key);
    }
}
