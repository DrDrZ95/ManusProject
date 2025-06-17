import React from 'react';
import { Monitor, Cpu, HardDrive } from 'lucide-react';

interface OSInfo {
  name: string;
  platform: string;
  userAgent: string;
  language: string;
  timezone: string;
  screenResolution: string;
  colorDepth: number;
  cookieEnabled: boolean;
  onlineStatus: boolean;
}

export const OSInfoDisplay: React.FC = () => {
  const [osInfo, setOsInfo] = React.useState<OSInfo | null>(null);

  React.useEffect(() => {
    const detectOSInfo = (): OSInfo => {
      const userAgent = navigator.userAgent;
      const platform = navigator.platform;
      
      let osName = 'Unknown OS';
      
      if (userAgent.indexOf('Win') !== -1) {
        osName = 'Windows';
      } else if (userAgent.indexOf('Mac') !== -1) {
        osName = 'macOS';
      } else if (userAgent.indexOf('Linux') !== -1) {
        osName = 'Linux';
      } else if (userAgent.indexOf('Android') !== -1) {
        osName = 'Android';
      } else if (userAgent.indexOf('iPhone') !== -1 || userAgent.indexOf('iPad') !== -1) {
        osName = 'iOS';
      }

      return {
        name: osName,
        platform: platform,
        userAgent: userAgent,
        language: navigator.language,
        timezone: Intl.DateTimeFormat().resolvedOptions().timeZone,
        screenResolution: `${screen.width}x${screen.height}`,
        colorDepth: screen.colorDepth,
        cookieEnabled: navigator.cookieEnabled,
        onlineStatus: navigator.onLine
      };
    };

    setOsInfo(detectOSInfo());

    // Listen for online/offline status changes
    const handleOnline = () => setOsInfo(prev => prev ? { ...prev, onlineStatus: true } : null);
    const handleOffline = () => setOsInfo(prev => prev ? { ...prev, onlineStatus: false } : null);

    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);

    return () => {
      window.removeEventListener('online', handleOnline);
      window.removeEventListener('offline', handleOffline);
    };
  }, []);

  if (!osInfo) return null;

  const getOSIcon = () => {
    switch (osInfo.name) {
      case 'Windows':
        return '🪟';
      case 'macOS':
        return '🍎';
      case 'Linux':
        return '🐧';
      case 'Android':
        return '🤖';
      case 'iOS':
        return '📱';
      default:
        return '💻';
    }
  };

  return (
    <div className="bg-white rounded-lg shadow-md p-4 border border-gray-200">
      <div className="flex items-center space-x-2 mb-3">
        <Monitor size={20} className="text-blue-600" />
        <h3 className="text-lg font-semibold text-gray-800">System Information</h3>
      </div>
      
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {/* Operating System */}
        <div className="flex items-center space-x-3 p-3 bg-gray-50 rounded-lg">
          <span className="text-2xl">{getOSIcon()}</span>
          <div>
            <p className="text-sm font-medium text-gray-600">Operating System</p>
            <p className="text-lg font-semibold text-gray-800">{osInfo.name}</p>
            <p className="text-xs text-gray-500">{osInfo.platform}</p>
          </div>
        </div>

        {/* Language & Timezone */}
        <div className="flex items-center space-x-3 p-3 bg-gray-50 rounded-lg">
          <span className="text-2xl">🌍</span>
          <div>
            <p className="text-sm font-medium text-gray-600">Locale</p>
            <p className="text-lg font-semibold text-gray-800">{osInfo.language}</p>
            <p className="text-xs text-gray-500">{osInfo.timezone}</p>
          </div>
        </div>

        {/* Screen Resolution */}
        <div className="flex items-center space-x-3 p-3 bg-gray-50 rounded-lg">
          <HardDrive size={24} className="text-green-600" />
          <div>
            <p className="text-sm font-medium text-gray-600">Display</p>
            <p className="text-lg font-semibold text-gray-800">{osInfo.screenResolution}</p>
            <p className="text-xs text-gray-500">{osInfo.colorDepth}-bit color</p>
          </div>
        </div>

        {/* Connection Status */}
        <div className="flex items-center space-x-3 p-3 bg-gray-50 rounded-lg">
          <div className={`w-3 h-3 rounded-full ${osInfo.onlineStatus ? 'bg-green-500' : 'bg-red-500'}`}></div>
          <div>
            <p className="text-sm font-medium text-gray-600">Connection</p>
            <p className="text-lg font-semibold text-gray-800">
              {osInfo.onlineStatus ? 'Online' : 'Offline'}
            </p>
            <p className="text-xs text-gray-500">
              Cookies: {osInfo.cookieEnabled ? 'Enabled' : 'Disabled'}
            </p>
          </div>
        </div>
      </div>

      {/* Deployment Info */}
      <div className="mt-4 p-3 bg-blue-50 rounded-lg border border-blue-200">
        <div className="flex items-center space-x-2 mb-2">
          <Cpu size={16} className="text-blue-600" />
          <p className="text-sm font-medium text-blue-800">Deployment Environment</p>
        </div>
        <p className="text-sm text-blue-700">
          React App running on {osInfo.name} ({osInfo.platform})
        </p>
        <p className="text-xs text-blue-600 mt-1">
          Built with Vite • Styled with Tailwind CSS
        </p>
      </div>
    </div>
  );
};

