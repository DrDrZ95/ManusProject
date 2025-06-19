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
      
      let osName = '未知系统';
      
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
    <div className="bg-white rounded-lg shadow-md p-4 border border-gray-200">
      <div className="flex items-center space-x-2 mb-3">
        <Monitor size={20} className="text-blue-600" />
        <h3 className="text-lg font-semibold text-gray-800">系统信息</h3>
      </div>
      
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {/* Operating System */}
        <div className="flex items-center space-x-3 p-3 bg-gray-50 rounded-lg">
          <span className="text-2xl">{getOSIcon()}</span>
          <div>
            <p className="text-sm font-medium text-gray-600">操作系统</p>
            <p className="text-lg font-semibold text-gray-800">{osInfo.name}</p>
            <p className="text-xs text-gray-500">{osInfo.platform}</p>
          </div>
        </div>

        {/* Language & Timezone */}
        <div className="flex items-center space-x-3 p-3 bg-gray-50 rounded-lg">
          <span className="text-2xl">🌍</span>
          <div>
            <p className="text-sm font-medium text-gray-600">区域设置</p>
            <p className="text-lg font-semibold text-gray-800">{osInfo.language}</p>
            <p className="text-xs text-gray-500">{osInfo.timezone}</p>
          </div>
        </div>
      </div>

      {/* Deployment Info */}
      <div className="mt-4 p-3 bg-blue-50 rounded-lg border border-blue-200">
        <div className="flex items-center space-x-2 mb-2">
          <span className="text-blue-600">🚀</span>
          <p className="text-sm font-medium text-blue-800">部署环境</p>
        </div>
        <p className="text-sm text-blue-700">
          React 应用运行在 {osInfo.name} ({osInfo.platform})
        </p>
        <p className="text-xs text-blue-600 mt-1">
          使用 Vite 构建 • Tailwind CSS 样式
        </p>
      </div>
    </div>
  );
};

