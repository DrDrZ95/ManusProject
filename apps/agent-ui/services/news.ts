
import { NewsItem } from '../types';
import { v4 as uuidv4 } from 'uuid';

const CACHE_DURATION = 6 * 60 * 60 * 1000; // 6 hours

const CATEGORIES = ['IT', 'Finance', 'AI'] as const;

const NEWS_SOURCES = [
  {
    category: 'AI',
    titles: [
      "DeepMind's new AlphaFold 3 predicts DNA structure",
      "OpenAI releases new reasoning model o1-preview",
      "Anthropic publishes research on constitutional AI",
      "NVIDIA announces new Blackwell GPU architecture for AI",
      "Meta releases Llama 3.1 405B open source model"
    ],
    summaries: [
      "A revolutionary leap in biological prediction accuracy.",
      "Enhanced capabilities in math and coding tasks.",
      "Focusing on safety and alignment in large language models.",
      "Pushing the boundaries of training performance.",
      "Setting a new standard for open weights models."
    ]
  },
  {
    category: 'IT',
    titles: [
      "Linux Kernel 6.11 adds initial support for Rust",
      "TypeScript 5.6 released with type narrowing improvements",
      "React 19 RC is now available for testing",
      "AWS introduces new Graviton4 instances",
      "Vercel launches v0 generative UI platform"
    ],
    summaries: [
      "Memory safety improvements coming to the kernel.",
      "Better developer experience and build speeds.",
      "Featuring the new compiler and actions API.",
      "Cost-effective performance for cloud workloads.",
      "Building interfaces with natural language prompts."
    ]
  },
  {
    category: 'Finance',
    titles: [
      "Bitcoin surges past $95k amid market optimism",
      "Fed signals potential rate cuts in Q4",
      "Tech stocks rally following earnings reports",
      "Global markets react to new trade policies",
      "Crypto ETF inflows hit record high"
    ],
    summaries: [
      "Cryptocurrency market sees renewed institutional interest.",
      "Economic indicators suggest cooling inflation.",
      "AI sector driving major index gains.",
      "Supply chain adjustments impacting global trade.",
      "Traditional finance continues integration with digital assets."
    ]
  }
];

// Function to generate deterministic but changing news based on time
export const generateNews = (): NewsItem[] => {
  const news: NewsItem[] = [];
  
  // Pick 3 distinct items
  const selectedCategories = [...CATEGORIES].sort(() => 0.5 - Math.random());

  selectedCategories.forEach((cat, index) => {
    const source = NEWS_SOURCES.find(s => s.category === cat)!;
    const randIndex = Math.floor(Math.random() * source.titles.length);
    
    news.push({
      id: uuidv4(),
      title: source.titles[randIndex],
      summary: source.summaries[randIndex],
      category: cat,
      url: `https://www.google.com/search?q=${encodeURIComponent(source.titles[randIndex])}`,
      thumbnailUrl: `https://picsum.photos/seed/${uuidv4()}/300/200`, // Random placeholder
      timestamp: Date.now()
    });
  });

  return news;
};

export const shouldFetchNews = (lastFetch: number): boolean => {
  return Date.now() - lastFetch > CACHE_DURATION;
};
