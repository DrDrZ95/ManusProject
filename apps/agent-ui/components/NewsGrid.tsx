
import React from 'react';
import { NewsItem } from '../types';
import { Icons } from './icons';
import clsx from 'clsx';

interface NewsGridProps {
  news: NewsItem[];
}

export const NewsGrid: React.FC<NewsGridProps> = ({ news }) => {
  if (news.length === 0) return null;

  return (
    <div className="grid grid-cols-1 md:grid-cols-3 gap-4 w-full max-w-4xl mt-8 px-4">
      {news.map((item) => (
        <a 
          key={item.id} 
          href={item.url}
          target="_blank"
          rel="noopener noreferrer"
          className="group block bg-white rounded-xl border border-gray-200 overflow-hidden hover:shadow-lg hover:-translate-y-1 transition-all duration-300 cursor-pointer"
        >
          <div className="h-32 bg-gray-100 overflow-hidden relative">
            <img 
              src={item.thumbnailUrl} 
              alt={item.title}
              className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-110 opacity-90 hover:opacity-100"
            />
            <div className="absolute top-2 right-2 bg-black/70 backdrop-blur-sm text-white text-[10px] font-bold px-2 py-0.5 rounded-full uppercase tracking-wider">
              {item.category}
            </div>
          </div>
          <div className="p-4">
            <div className="flex items-start justify-between gap-2 mb-2">
               <h3 className="text-sm font-bold text-gray-900 leading-tight group-hover:text-blue-600 transition-colors line-clamp-2">
                 {item.title}
               </h3>
               <Icons.ArrowRight className="w-4 h-4 text-gray-300 group-hover:text-blue-500 -rotate-45 group-hover:rotate-0 transition-transform" />
            </div>
            <p className="text-xs text-gray-500 line-clamp-2">
              {item.summary}
            </p>
          </div>
        </a>
      ))}
    </div>
  );
};
