import React from 'react';
import { Monitor } from 'lucide-react';

interface OSInfo {
  name: string;
  platform: string;
  language: string;
  timezone: string;
}

export const OSInfoDisplay: React.FC = () => {
  const [osInfo, setOsInfo] = React.useState<OSInfo | null>(null);

  React.useEffect(() => {
    const detectOSInfo = (): OSInfo => {
      const userAgent = navigator.userAgent;
      const platform = navigator.platform;
      
      let osName = 'Unknown System';
      
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
        language: navigator.language,
        timezone: Intl.DateTimeFormat().resolvedOptions().timeZone
      };
    };

    setOsInfo(detectOSInfo());
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
    <div className="bg-white rounded-lg shadow-sm p-3 border border-gray-300">
      <div className="flex items-center space-x-2 mb-2">
        <Monitor size={16} className="text-gray-600" />
        <h3 className="text-sm font-semibold text-gray-800">System Information</h3>
      </div>
      
      <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
        {/* Operating System */}
        <div className="flex items-center space-x-2 p-2 bg-gray-50 rounded">
          <span className="text-lg">{getOSIcon()}</span>
          <div>
            <p className="text-xs font-medium text-gray-600">Operating System</p>
            <p className="text-sm font-semibold text-gray-800">{osInfo.name}</p>
          </div>
        </div>

        {/* Language & Timezone */}
        <div className="flex items-center space-x-2 p-2 bg-gray-50 rounded">
          <span className="text-lg">🌍</span>
          <div>
            <p className="text-xs font-medium text-gray-600">Locale</p>
            <p className="text-sm font-semibold text-gray-800">{osInfo.language}</p>
          </div>
        </div>
      </div>

      {/* Deployment Info - Compact */}
      <div className="mt-2 p-2 bg-gray-50 rounded border border-gray-200">
        <div className="flex items-center space-x-1 mb-1">
          <span className="text-gray-600 text-sm">🚀</span>
          <p className="text-xs font-medium text-gray-700">Deployment Environment</p>
        </div>
        <p className="text-xs text-gray-600">
          React App running on {osInfo.name} ({osInfo.platform})
        </p>
        <p className="text-xs text-gray-500 mt-1">
          Built with Vite • Styled with Tailwind CSS
        </p>
      </div>
    </div>
  );
};

