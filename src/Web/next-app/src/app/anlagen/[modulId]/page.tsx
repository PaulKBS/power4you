import React from 'react';
import ClientModulePage from './client-page';

export default function ModuleDetailPage({ params }: { params: { modulId: string } }) {
  // No need to use React.use() on params, it's already available
  const modulId = params.modulId;
  
  // Pass the modulId to the client component
  return (
    <ClientModulePage modulId={modulId} />
  );
} 