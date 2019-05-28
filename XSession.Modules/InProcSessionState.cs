using System.Web;
using System.Web.SessionState;

namespace XSession.Modules
{
    internal sealed class InProcSessionState
    {
        internal ISessionStateItemCollection Items;
        internal HttpStaticObjectsCollection StaticObjects;
        internal int Timeout;

        internal InProcSessionState(SessionStateStoreData data)
        {
            this.Items = data.Items;
            this.StaticObjects = data.StaticObjects;
            this.Timeout = data.Timeout;
        }

        internal SessionStateStoreData ToStoreData(HttpContext context)
        {
            return SessionUtils.CreateLegitStoreData(context, this.Items, this.StaticObjects, this.Timeout);
        }
    }
}
