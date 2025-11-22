
import React from 'react';
import { Icons } from './icons';
import { Link } from 'react-router-dom';

export const NotFoundPage: React.FC = () => {
  return (
    <div className="min-h-screen w-full flex flex-col items-center justify-center bg-[#F3F4F6] text-gray-900 p-4 relative overflow-hidden font-sans">
       {/* Ambient Background */}
      <div className="absolute top-0 left-0 w-full h-full overflow-hidden z-0 pointer-events-none">
        <div className="absolute bottom-0 left-0 w-full h-1/2 bg-gradient-to-t from-gray-200/30 to-transparent"></div>
        <div className="absolute top-1/4 left-1/4 w-96 h-96 bg-gray-200 rounded-full mix-blend-multiply filter blur-3xl opacity-30"></div>
      </div>

      <div className="z-10 flex flex-col items-center text-center">
        <div className="relative mb-8">
            <div className="w-24 h-24 bg-black text-white rounded-3xl flex items-center justify-center shadow-2xl rotate-6 z-10 relative">
                <span className="text-4xl font-mono font-bold">404</span>
            </div>
            <div className="absolute top-0 left-0 w-24 h-24 bg-gray-300 rounded-3xl -rotate-6 opacity-50 z-0"></div>
        </div>

        <h1 className="text-3xl font-bold mb-3 tracking-tight">Page Not Found</h1>
        <p className="text-gray-500 mb-8 max-w-md leading-relaxed">
          The path you are attempting to access does not exist within this Agent workspace container.
        </p>
        
        <Link 
          to="/"
          className="flex items-center gap-3 px-8 py-3.5 bg-white border border-gray-200 text-gray-900 rounded-xl font-medium hover:bg-gray-50 hover:border-gray-300 transition-all shadow-sm group"
        >
          <Icons.ArrowRight className="w-4 h-4 rotate-180 text-gray-400 group-hover:text-black transition-colors" />
          <span>Return to Agent</span>
        </Link>
      </div>
    </div>
  );
};
