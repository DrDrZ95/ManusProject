import { useState } from 'react';
import { Terminal } from 'lucide-react';
import { Sidebar } from './components/Sidebar';
import { ChatArea } from './components/ChatArea';
import { TerminalComponent } from './components/TerminalComponent';
import { OSInfoDisplay } from './components/OSInfoDisplay';
import { ChatProvider } from './contexts/ChatContext';
import './App.css';

function App() {
  const [isMobileSidebarOpen, setIsMobileSidebarOpen] = useState(false);
  const [isTerminalVisible, setIsTerminalVisible] = useState(false);

  return (
    <ChatProvider>
      <div className="flex h-screen bg-gradient-to-br from-blue-50 to-indigo-100">
        {/* Mobile sidebar toggle */}
        <button
          className="md:hidden fixed top-4 left-4 z-20 p-2 bg-gradient-to-r from-blue-500 to-indigo-600 text-white rounded-md shadow-lg"
          onClick={() => setIsMobileSidebarOpen(!isMobileSidebarOpen)}
        >
          {isMobileSidebarOpen ? '✕' : '☰'}
        </button>

        {/* Terminal toggle button */}
        <button
          className="fixed top-4 right-4 z-20 p-2 bg-gray-800 text-green-400 rounded-md hover:bg-gray-700 transition-colors shadow-lg"
          onClick={() => setIsTerminalVisible(!isTerminalVisible)}
          title="切换终端"
        >
          <Terminal size={20} />
        </button>

        {/* Sidebar - hidden on mobile unless toggled */}
        <div 
          className={`
            md:relative fixed inset-y-0 left-0 z-10 transform 
            ${isMobileSidebarOpen ? 'translate-x-0' : '-translate-x-full'} 
            md:translate-x-0 transition-transform duration-300 ease-in-out
          `}
        >
          <Sidebar />
        </div>

        {/* Main content area */}
        <div className="flex-1 flex flex-col overflow-hidden">
          {/* OS Info Display */}
          <div className="p-4 border-b border-indigo-200 bg-white bg-opacity-80 backdrop-blur-sm">
            <OSInfoDisplay />
          </div>
          
          {/* Chat Area */}
          <div className="flex-1">
            <ChatArea />
          </div>
        </div>

        {/* Terminal Component */}
        <TerminalComponent 
          isVisible={isTerminalVisible} 
          onToggle={() => setIsTerminalVisible(!isTerminalVisible)} 
        />
      </div>
    </ChatProvider>
  );
}

export default App;

