using System;
using System.Reflection;
using System.Collections;
using System.Web;
using System.Security.Permissions;
using System.ComponentModel;

namespace SharpPieces.Web
{

    /// <summary>
    /// Wrapper over the internal class Syatem.Web.UI.DataSourceHelper.
    /// </summary>
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal), AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
    public sealed class DataSourceHelper
    {

        // fields

        private static Type internalDataSourceHelper = null;
        private static MethodInfo internalGetResolvedDataSource = null;

        // methods

        /// <summary>
        /// Gets the IEnumerable resolved data source.
        /// </summary>
        /// <param name="dataSource">The data source.</param>
        /// <param name="dataMember">The data member.</param>
        /// <returns>The IEnumerable resolved data source.</returns>
        public static IEnumerable GetResolvedDataSource(object dataSource, string dataMember)
        {
            if (null == internalDataSourceHelper)
            {
                Assembly systemWeb = System.Reflection.Assembly.GetAssembly(typeof(System.Web.UI.AttributeCollection));
                DataSourceHelper.internalDataSourceHelper = systemWeb.GetType("System.Web.UI.DataSourceHelper");
            }
            if (null == internalGetResolvedDataSource)
            {
                DataSourceHelper.internalGetResolvedDataSource = DataSourceHelper.internalDataSourceHelper.GetMethod("GetResolvedDataSource", BindingFlags.NonPublic | BindingFlags.Static);
            }

            //object result = DataSourceHelper.internalGetResolvedDataSource.Invoke(null, new object[] { dataSource, dataMember });
            object result = DataSourceHelper.GetResolvedDataSourceInternal(dataSource, dataMember);

            return (result is IEnumerable) ? (IEnumerable)result : null;
        }

        internal static IEnumerable GetResolvedDataSourceInternal(object dataSource, string dataMember)
        {
            if (dataSource != null)
            {
                IListSource source = dataSource as IListSource;
                if (source != null)
                {
                    IList list = source.GetList();
                    if (!source.ContainsListCollection)
                    {
                        return list;
                    }
                    if ((list != null) && (list is ITypedList))
                    {
                        PropertyDescriptorCollection itemProperties = ((ITypedList)list).GetItemProperties(new PropertyDescriptor[0]);
                        if ((itemProperties == null) || (itemProperties.Count == 0))
                        {
                            throw new HttpException("ListSource_Without_DataMembers");
                        }
                        PropertyDescriptor descriptor = null;
                        if (string.IsNullOrEmpty(dataMember))
                        {
                            descriptor = itemProperties[0];
                        }
                        else
                        {
                            descriptor = itemProperties.Find(dataMember, true);
                        }
                        if (descriptor != null)
                        {
                            object component = list[0];
                            object obj3 = descriptor.GetValue(component);
                            if ((obj3 != null) && (obj3 is IEnumerable))
                            {
                                return (IEnumerable)obj3;
                            }
                        }
                        throw new HttpException("ListSource_Missing_DataMember");
                    }
                }
                if (dataSource is IEnumerable)
                {
                    return (IEnumerable)dataSource;
                }
            }
            return null;
        }


    }

}
