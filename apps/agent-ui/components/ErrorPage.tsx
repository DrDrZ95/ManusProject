
import React from 'react';
import { Icons } from './icons';

export const ErrorPage: React.FC = () => {
  const handleReload = () => {
    // Force a hard reload to clear potential bad state and return to root
    window.location.href = '/';
  };

  return (
    <div className="min-h-screen w-full flex flex-col items-center justify-center bg-[#F3F4F6] text-gray-900 p-4 relative overflow-hidden font-sans">
      {/* Background decoration */}
      <div className="absolute top-0 left-0 w-full h-full overflow-hidden z-0 pointer-events-none opacity-50">
        <div className="absolute -top-20 -right-20 w-96 h-96 bg-red-100 rounded-full mix-blend-multiply filter blur-3xl opacity-30"></div>
      </div>

      <div className="z-10 flex flex-col items-center text-center max-w-md">
        <div className="w-20 h-20 bg-white text-red-500 rounded-2xl flex items-center justify-center shadow-xl border border-red-100 mb-8">
            <Icons.Close className="w-10 h-10" />
        </div>
        
        <h1 className="text-3xl font-bold mb-3 tracking-tight text-gray-900">System Error</h1>
        
        <p className="text-gray-500 mb-8 leading-relaxed">
          The Agent workspace encountered an unexpected issue. This could be due to a network interruption or a simulated crash.
        </p>
        
        <button 
          onClick={handleReload}
          className="flex items-center gap-2 px-8 py-3.5 bg-black text-white rounded-xl font-medium hover:bg-gray-800 transition-all shadow-lg hover:scale-105 active:scale-95"
        >
          <Icons.Zap className="w-4 h-4" />
          <span>Reload Workspace</span>
        </button>
        
        <div className="mt-8 text-xs text-gray-400 font-mono">
          Error Code: AGENT_RUNTIME_EXCEPTION
        </div>
      </div>
    </div>
  );
};
