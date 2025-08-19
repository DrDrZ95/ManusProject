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
      <div className="flex h-screen bg-white overflow-hidden">
        {/* Mobile sidebar toggle */}
        <button
          className="md:hidden fixed top-4 left-4 z-20 p-2 bg-gradient-to-r from-gray-500 to-gray-600 text-white rounded-md shadow-lg"
          onClick={() => setIsMobileSidebarOpen(!isMobileSidebarOpen)}
        >
          {isMobileSidebarOpen ? '✕' : '☰'}
        </button>

        {/* Terminal toggle button */}
        <button
          className="fixed top-4 right-4 z-20 p-2 bg-white text-gray-600 rounded-md hover:bg-gray-50 transition-colors shadow-lg border border-gray-300"
          onClick={() => setIsTerminalVisible(!isTerminalVisible)}
          title="Toggle Terminal"
        >
          <Terminal size={20} />
        </button>

        {/* Sidebar - hidden on mobile unless toggled */}
        <div 
          className={`
            md:relative fixed inset-y-0 left-0 z-10 transform 
            ${isMobileSidebarOpen ? 'translate-x-0' : '-translate-x-full'} 
            md:translate-x-0 transition-transform duration-300 ease-in-out
            w-80 md:w-80 flex-shrink-0
          `}
        >
          <Sidebar />
        </div>

        {/* Main content area - Full height, Claude-like layout */}
        <div className="flex-1 flex flex-col h-screen overflow-hidden bg-white">
          {/* OS Info Display - Minimal header */}
          <div className="px-6 py-2 border-b border-gray-200 bg-white flex-shrink-0">
            <OSInfoDisplay />
          </div>
          
          {/* Chat Area - Takes all remaining space */}
          <div className="flex-1 overflow-hidden">
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

