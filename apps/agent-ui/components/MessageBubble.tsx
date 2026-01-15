
import React, { useState } from 'react';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';
import { Prism as SyntaxHighlighter } from 'react-syntax-highlighter';
import { vs } from 'react-syntax-highlighter/dist/esm/styles/prism';
import { Message, Role } from '../types';
import { Icons } from './icons';
import clsx from 'clsx';
import { motion, AnimatePresence } from 'framer-motion';
import { useStore } from '../store';
import { translations } from '../locales';

interface MessageBubbleProps {
  message: Message;
  isLast: boolean;
}

const CodeBlock = ({ language, children }: { language: string, children?: React.ReactNode }) => {
  const [copied, setCopied] = useState(false);
  const lang = useStore(s => s.language);
  const t = translations[lang];

  const handleCopy = () => {
    navigator.clipboard.writeText(String(children));
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <div className="relative group my-4 rounded-lg overflow-hidden border border-gray-200 shadow-sm">
      <div className="flex items-center justify-between px-4 py-2 bg-gray-50 border-b border-gray-200">
        <span className="text-xs font-mono text-gray-600">{language || 'code'}</span>
        <button 
          onClick={handleCopy}
          className="flex items-center space-x-1 text-xs text-gray-500 hover:text-black transition-colors opacity-0 group-hover:opacity-100"
        >
          {copied ? <Icons.Check className="w-3 h-3 text-green-500" /> : <Icons.Copy className="w-3 h-3" />}
          <span>{copied ? t.copied : t.copy}</span>
        </button>
      </div>
      <SyntaxHighlighter
        style={vs}
        language={language}
        PreTag="div"
        customStyle={{
          margin: 0,
          borderRadius: 0,
          background: '#ffffff',
          fontSize: '0.875rem',
          padding: '1rem',
        }}
      >
        {String(children).replace(/\n$/, '')}
      </SyntaxHighlighter>
    </div>
  );
};

export const MessageBubble: React.FC<MessageBubbleProps> = ({ message, isLast }) => {
  const isUser = message.role === Role.USER;
  const userAvatar = useStore(s => s.user?.avatar);
  const [isHovered, setIsHovered] = useState(false);
  const [copied, setCopied] = useState(false);

  const handleCopyMessage = () => {
    navigator.clipboard.writeText(message.content);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };
  
  return (
    <motion.div 
      initial={{ opacity: 0, y: 10 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.3 }}
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
      className={clsx(
        "flex w-full mb-6 relative group",
        isUser ? "justify-end" : "justify-start"
      )}
    >
      <div className={clsx(
        "flex max-w-[85%] md:max-w-[75%]",
        isUser ? "flex-row-reverse" : "flex-row"
      )}>
        {/* Avatar */}
        <div className={clsx(
          "flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center mt-1 shadow-sm overflow-hidden",
          isUser ? "bg-gray-200 ml-3" : "bg-black text-white mr-3"
        )}>
          {isUser ? (
             userAvatar ? (
               <img src={userAvatar} className="w-full h-full object-cover filter grayscale" />
             ) : (
               <div className="w-full h-full rounded-full bg-gradient-to-br from-gray-100 to-gray-300" />
             )
          ) : (
             <Icons.Zap className="w-4 h-4" fill="currentColor" />
          )}
        </div>

        {/* Content */}
        <div className={clsx(
          "flex flex-col",
          isUser ? "items-end" : "items-start",
          "min-w-0 flex-1"
        )}>
          {/* Attachments if any */}
          {message.attachments && message.attachments.length > 0 && (
             <div className={clsx("flex flex-wrap gap-2 mb-2", isUser ? "justify-end" : "justify-start")}>
                 {message.attachments.map((att, idx) => (
                     <div key={idx} className="flex items-center gap-2 bg-white px-3 py-2 rounded-lg border border-gray-200 shadow-sm text-sm">
                        <Icons.Attach className="w-4 h-4 text-gray-500" />
                        <span className="text-gray-700 max-w-[200px] truncate">{att.name}</span>
                     </div>
                 ))}
             </div>
          )}

          <div className={clsx(
            "relative text-[15px] leading-relaxed max-w-full overflow-hidden",
            isUser 
              ? "px-5 py-3.5 bg-white border border-gray-200 text-gray-900 rounded-2xl rounded-tr-sm shadow-sm" 
              : "py-1 px-0 bg-transparent text-gray-900 w-full" 
          )}>
             {isUser ? (
               <div className="whitespace-pre-wrap break-words">{message.content}</div>
             ) : (
               <div className="prose prose-sm max-w-none prose-headings:font-semibold prose-p:text-gray-800 prose-a:text-blue-600 prose-code:text-red-500 prose-code:bg-gray-100 prose-code:px-1 prose-code:py-0.5 prose-code:rounded-md prose-pre:p-0 prose-pre:bg-transparent break-words">
                  <ReactMarkdown
                    remarkPlugins={[remarkGfm]}
                    components={{
                      code({ node, inline, className, children, ...props }: any) {
                        const match = /language-(\w+)/.exec(className || '');
                        return !inline && match ? (
                          <CodeBlock language={match[1]}>{children}</CodeBlock>
                        ) : (
                          <code className={className} {...props}>
                            {children}
                          </code>
                        );
                      }
                    }}
                  >
                    {message.content || ''} 
                  </ReactMarkdown>
                  {message.isStreaming && (
                      <span className="inline-block w-1.5 h-4 ml-1 align-middle bg-black animate-pulse" />
                  )}
               </div>
             )}
          </div>
          
          {/* Timestamp & Actions */}
          <div className={clsx(
             "flex items-center gap-3 mt-1 px-1",
             isUser ? "flex-row-reverse" : "flex-row"
          )}>
             <span className="text-[10px] text-gray-400">
                {new Date(message.timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
             </span>

             <AnimatePresence>
                {isHovered && !message.isStreaming && (
                    <motion.div 
                        initial={{ opacity: 0, y: 5 }}
                        animate={{ opacity: 1, y: 0 }}
                        exit={{ opacity: 0, y: 5 }}
                        className="flex items-center gap-1.5"
                    >
                        <button 
                            onClick={handleCopyMessage}
                            title="Copy message"
                            className="p-1 text-gray-400 hover:text-black hover:bg-gray-200 rounded-md transition-all"
                        >
                            {copied ? <Icons.Check className="w-3 h-3 text-green-600" /> : <Icons.Copy className="w-3 h-3" />}
                        </button>
                        <button 
                            title="Share"
                            className="p-1 text-gray-400 hover:text-black hover:bg-gray-200 rounded-md transition-all"
                        >
                            <Icons.Mail className="w-3 h-3" />
                        </button>
                    </motion.div>
                )}
             </AnimatePresence>
          </div>
        </div>
      </div>
    </motion.div>
  );
};
