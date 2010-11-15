using System;
using System.Web;
using System.Web.Mvc;
using Castle.Windsor;
using System.Web.Routing;

namespace RavenDBMembership.Web.Infrastructure
{
	/// <summary>
	/// Controller Factory class for instantiating controllers using the Windsor IoC container.
	/// Copied from MvcContrib (http://mvccontrib.googlecode.com/svn/trunk).
	/// </summary>
	public class WindsorControllerFactory : DefaultControllerFactory
	{
		private readonly IWindsorContainer container;

		/// <summary>
		/// Creates a new instance of the <see cref="WindsorControllerFactory"/> class.
		/// </summary>
		/// <param name="container">The Windsor container instance to use when creating controllers.</param>
		public WindsorControllerFactory(IWindsorContainer container)
		{
			if (container == null)
			{
				throw new ArgumentNullException("container");
			}
			this.container = container;
		}

		protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
		{
			if (controllerType == null)
			{
				throw new HttpException(404, string.Format("The controller for path '{0}' could not be found or it does not implement IController.", requestContext.HttpContext.Request.Path));
			}

			return (IController)container.Resolve(controllerType);
		}

		public override void ReleaseController(IController controller)
		{
			var disposable = controller as IDisposable;

			if (disposable != null)
			{
				disposable.Dispose();
			}

			container.Release(controller);
		}
	}
}