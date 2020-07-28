/*
 * xPilot: X-Plane pilot client for VATSIM
 * Copyright (C) 2019-2020 Justin Shannon
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see http://www.gnu.org/licenses/.
*/
using XPilot.PilotClient.AudioForVatsim;
using XPilot.PilotClient.Config;
using XPilot.PilotClient.Network;
using XPilot.PilotClient.XplaneAdapter;
using Appccelerate.EventBroker;
using Ninject.Extensions.Factory;
using Ninject.Modules;
using XPilot.PilotClient.Network.Aircraft;
using XPilot.PilotClient.Network.Controllers;

namespace XPilot.PilotClient.Core
{
    public class InjectionModules : NinjectModule
    {
        public override void Load()
        {
            Bind<IUserInterface>().ToFactory();
            Bind<ITabPages>().ToFactory();
            Bind<IEventBroker>().To(typeof(EventBroker)).InSingletonScope();
            Bind<IAppConfig>().To(typeof(AppConfig)).InSingletonScope();
            Bind<IFsdManger>().To(typeof(FsdManager)).InSingletonScope();
            Bind<IAfvManager>().To(typeof(AfvManager)).InSingletonScope();
            Bind<ISoundManager>().To(typeof(SoundManager)).InSingletonScope();
            Bind<IXplaneConnectionManager>().To(typeof(XplaneConnectionManager)).InSingletonScope();
            Bind<IVersionCheck>().To(typeof(VersionCheck)).InSingletonScope();
            Bind<IUserAircraftManager>().To(typeof(UserAircraftManager)).InSingletonScope();
            Bind<ISelcalGenerator>().To(typeof(SelcalGenerator)).InSingletonScope();
            Bind<INetworkAircraftManager>().To(typeof(NetworkAircraftManager)).InSingletonScope();
            Bind<IPttManager>().To(typeof(PttManager)).InSingletonScope();
            Bind<IControllerManager>().To(typeof(ControllerManager)).InSingletonScope();
            Bind<IControllerAtisManager>().To(typeof(ControllerAtisManager)).InSingletonScope();
        }
    }
}
