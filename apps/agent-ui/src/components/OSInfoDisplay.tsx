import React from 'react';

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
    <div className="flex items-center justify-between text-sm text-gray-600">
      {/* Minimal system info */}
      <div className="flex items-center space-x-4">
        <div className="flex items-center space-x-1">
          <span className="text-base">{getOSIcon()}</span>
          <span className="font-medium">{osInfo.name}</span>
        </div>
        <div className="flex items-center space-x-1">
          <span>🌍</span>
          <span>{osInfo.language}</span>
        </div>
      </div>
      
      {/* Deployment status */}
      <div className="flex items-center space-x-1 text-xs text-gray-500">
        <span className="w-2 h-2 bg-green-500 rounded-full"></span>
        <span>AgentUI Active</span>
      </div>
    </div>
  );
};

