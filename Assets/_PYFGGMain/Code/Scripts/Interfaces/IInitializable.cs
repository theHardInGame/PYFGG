using UnityEngine;
using System.Threading.Tasks;
using System.Threading;

public interface IInitializable
{
    Task InitializeAsync();
}
