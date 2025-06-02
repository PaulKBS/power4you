import React from 'react';
import ClientModulePage from './client-page';

export default async function ModuleDetailPage({ params }: { params: Promise<{ modulId: string }> }) {
  // Await the params promise in Next.js 15
  const { modulId } = await params;
  
  // Pass the modulId to the client component
  return (
    <ClientModulePage modulId={modulId} />
  );
} 