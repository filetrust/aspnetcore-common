namespace Glasswall.Common.Storage.Store
{
    public interface IPathActions
    {
        PathAction DecideAction(string path);
    }
}