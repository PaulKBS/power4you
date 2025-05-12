import type { Metadata } from 'next';

export const metadata: Metadata = {
  title: 'API Dokumentation | Power4You',
  description: 'Dokumentation der Power4You API für Entwickler',
};

export default function ApiDocLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <main className="min-h-screen bg-white dark:bg-gray-900 transition-colors duration-300">
      <div className="py-6">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <h1 className="text-3xl font-semibold text-gray-900 dark:text-white">
            API Dokumentation
          </h1>
          <p className="mt-2 text-sm text-gray-600 dark:text-gray-200">
            Komplette Dokumentation der Power4You API für Entwickler
          </p>
        </div>
      </div>
      <div className="border-t border-gray-200 dark:border-gray-800">
        {children}
      </div>
    </main>
  );
} 