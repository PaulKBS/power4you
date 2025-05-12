'use client';

import { useEffect, useState } from 'react';
import SwaggerUI from 'swagger-ui-react';
import 'swagger-ui-react/swagger-ui.css';

// Basic OpenAPI specification type
interface OpenAPISpec {
  openapi: string;
  info: {
    title: string;
    version: string;
    description?: string;
  };
  paths: Record<string, unknown>;
  components?: Record<string, unknown>;
  tags?: Array<{ name: string; description?: string }>;
  [key: string]: unknown;
}

export default function ApiDoc() {
  const [spec, setSpec] = useState<OpenAPISpec | null>(null);

  useEffect(() => {
    async function fetchSpec() {
      const response = await fetch('/api/swagger');
      const data = await response.json();
      setSpec(data);
    }
    fetchSpec();
  }, []);

  if (!spec) {
    return (
      <div className="p-6 max-w-7xl mx-auto">
        <div className="text-center py-12">
          <div className="inline-block h-8 w-8 animate-spin rounded-full border-4 border-solid border-orange-500 border-r-transparent"></div>
          <p className="mt-4 text-gray-600 dark:text-gray-300">Loading API documentation...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="swagger-container">
      <SwaggerUI spec={spec} />
      <style jsx global>{`
        /* Base styling */
        .swagger-ui {
          font-family: system-ui, -apple-system, BlinkMacSystemFont, sans-serif;
        }
        
        /* Light mode styling */
        .swagger-ui .info .title {
          color: #f97316;
        }
        
        .swagger-ui .opblock.opblock-get .opblock-summary-method {
          background-color: #3b82f6;
        }
        
        .swagger-ui .opblock.opblock-put .opblock-summary-method {
          background-color: #f59e0b;
        }
        
        .swagger-ui .opblock.opblock-post .opblock-summary-method {
          background-color: #10b981;
        }
        
        .swagger-ui .opblock.opblock-delete .opblock-summary-method {
          background-color: #ef4444;
        }
        
        .swagger-ui .btn.execute {
          background-color: #f97316;
          border-color: #f97316;
        }
        
        .swagger-ui .btn.authorize {
          background-color: #10b981;
          border-color: #10b981;
        }
        
        /* Container and layout */
        .swagger-container {
          margin: 20px auto;
          max-width: 1200px;
          padding: 0 20px;
        }
        
        /* Dark mode styling */
        html.dark body {
          background-color: #1e293b;
          color: #f1f5f9; /* Lighter text color */
        }
        
        /* Button styling for dark mode */
        html.dark .swagger-ui .btn {
          background-color: #334155;
          color: #f1f5f9;
          border-color: #475569;
        }
        
        html.dark .swagger-ui .btn.execute {
          background-color: #f97316;
          color: white;
          border-color: #ea580c;
        }
        
        html.dark .swagger-ui .btn.authorize {
          background-color: #10b981;
          color: white;
          border-color: #059669;
        }
        
        html.dark .swagger-ui .btn.cancel {
          background-color: #6b7280;
          color: white;
          border-color: #4b5563;
        }
        
        html.dark .swagger-ui .btn:hover {
          background-color: #475569;
        }
        
        html.dark .swagger-ui .btn.execute:hover {
          background-color: #ea580c;
        }
        
        html.dark .swagger-ui .btn.authorize:hover {
          background-color: #059669;
        }
        
        html.dark .swagger-ui .btn.cancel:hover {
          background-color: #4b5563;
        }
        
        /* Additional button types and controls in dark mode */
        html.dark .swagger-ui .try-out__btn {
          background-color: #334155;
          color: #f1f5f9;
          border-color: #475569;
        }
        
        html.dark .swagger-ui .try-out__btn:hover {
          background-color: #475569;
        }
        
        html.dark .swagger-ui .opblock-control-arrow {
          filter: invert(0.8);
        }
        
        html.dark .swagger-ui select {
          background-color: #334155;
          color: #f1f5f9;
        }
        
        html.dark .swagger-ui .download-url-wrapper .select-label {
          color: #f1f5f9;
        }
        
        html.dark .swagger-ui .download-url-button {
          background-color: #334155;
          color: #f1f5f9;
          border-color: #475569;
        }
        
        html.dark .swagger-ui .download-url-button:hover {
          background-color: #475569;
        }
        
        html.dark .swagger-ui .parameters-container .parameters-col_description .parameter__enum {
          color: #cbd5e1 !important;
        }
        
        html.dark .swagger-ui input[type=text],
        html.dark .swagger-ui .operation-tag-content input[type=text] {
          border-color: #475569;
        }
        
        html.dark .swagger-ui .tab li button.tablinks {
          color: #cbd5e1;
        }
        
        html.dark .swagger-ui .tab li button.tablinks.active {
          color: #f97316;
        }
        
        html.dark .swagger-ui .expand-operation svg {
          fill: #f1f5f9;
        }
        
        html.dark .swagger-ui .opblock-summary-control:focus {
          outline-color: #f97316;
        }
        
        html.dark .swagger-ui .auth-container .authorize {
          color: white;
        }
        
        html.dark .swagger-ui .copy-to-clipboard {
          background-color: #334155;
        }
        
        html.dark .swagger-ui .copy-to-clipboard button {
          background-color: #475569;
          color: #f1f5f9;
        }
        
        html.dark .swagger-ui .model-toggle:after {
          background-color: #cbd5e1;
        }
        
        html.dark .swagger-ui,
        html.dark .swagger-ui .info .title,
        html.dark .swagger-ui h1, 
        html.dark .swagger-ui h2, 
        html.dark .swagger-ui h3, 
        html.dark .swagger-ui h5,
        html.dark .swagger-ui .info p,
        html.dark .swagger-ui .info li,
        html.dark .swagger-ui .info table,
        html.dark .swagger-ui table thead tr th,
        html.dark .swagger-ui table tbody tr td,
        html.dark .swagger-ui .parameter__name,
        html.dark .swagger-ui .parameter__type,
        html.dark .swagger-ui .parameter__deprecated,
        html.dark .swagger-ui .parameter__in,
        html.dark .swagger-ui .parameter__enum,
        html.dark .swagger-ui label,
        html.dark .swagger-ui .opblock-tag,
        html.dark .swagger-ui .opblock .opblock-summary-operation-id,
        html.dark .swagger-ui .opblock .opblock-summary-path, 
        html.dark .swagger-ui .opblock .opblock-summary-path__deprecated,
        html.dark .swagger-ui .opblock-description-wrapper p,
        html.dark .swagger-ui .responses-inner h4,
        html.dark .swagger-ui .responses-inner h5,
        html.dark .swagger-ui .response-col_status,
        html.dark .swagger-ui .response-col_description,
        html.dark .swagger-ui .model-title,
        html.dark .swagger-ui .model {
          color: #f1f5f9; /* Lighter color for better visibility */
        }
        
        html.dark .swagger-ui .info {
          background-color: #1e293b;
        }
        
        html.dark .swagger-ui .scheme-container {
          background-color: #0f172a;
        }
        
        html.dark .swagger-ui .opblock {
          background-color: #1e293b;
          border-color: #334155;
        }
        
        html.dark .swagger-ui .opblock .opblock-summary {
          border-color: #334155;
        }
        
        html.dark .swagger-ui .opblock .opblock-section-header {
          background-color: #0f172a;
          border-color: #334155;
        }
        
        html.dark .swagger-ui .opblock-tag {
          border-color: #334155;
        }
        
        html.dark .swagger-ui table {
          background-color: #1e293b;
          border-color: #334155;
        }
        
        html.dark .swagger-ui input,
        html.dark .swagger-ui select,
        html.dark .swagger-ui textarea {
          background-color: #0f172a;
          color: #f1f5f9; /* Lighter color for better visibility */
          border-color: #334155;
        }
        
        html.dark .swagger-ui section.models {
          background-color: #1e293b;
          border-color: #334155;
        }
        
        html.dark .swagger-ui section.models h4 {
          color: #f8fafc;
        }
        
        html.dark .swagger-ui .model-container {
          background-color: #0f172a;
          border-color: #334155;
        }
        
        html.dark .swagger-ui .model-box-control,
        html.dark .swagger-ui .models-control {
          color: #f1f5f9; /* Lighter color for better visibility */
        }
        
        html.dark .swagger-ui .dialog-ux .modal-ux {
          background-color: #1e293b;
          border-color: #334155;
        }
        
        html.dark .swagger-ui .dialog-ux .modal-ux-header h3 {
          color: #f8fafc;
        }
        
        html.dark .swagger-ui .dialog-ux .modal-ux-content {
          color: #f1f5f9; /* Lighter color for better visibility */
        }
        
        html.dark .swagger-ui .markdown code, 
        html.dark .swagger-ui .renderedMarkdown code {
          background-color: #0f172a;
          color: #e2e8f0; /* Lighter color for better visibility */
        }
        
        html.dark .swagger-ui .highlight-code {
          background-color: #0f172a;
        }
        
        html.dark .swagger-ui .highlight-code .token.punctuation,
        html.dark .swagger-ui .highlight-code .token.operator {
          color: #cbd5e1; /* Lighter color for better visibility */
        }
        
        html.dark .swagger-ui .highlight-code .token.string, 
        html.dark .swagger-ui .highlight-code .token.boolean,
        html.dark .swagger-ui .highlight-code .token.number {
          color: #f97316;
        }
        
        html.dark .swagger-ui .highlight-code .token.keyword {
          color: #3b82f6;
        }
        
        /* Additional enhancement for paragraph text and descriptions */
        html.dark .swagger-ui p,
        html.dark .swagger-ui .opblock-description-wrapper p,
        html.dark .swagger-ui .opblock-external-docs-wrapper p,
        html.dark .swagger-ui .opblock-title_normal p {
          color: #f1f5f9 !important; /* Lighter color with !important to override inline styles */
        }
        
        /* Make description text more visible */
        html.dark .swagger-ui .markdown p, 
        html.dark .swagger-ui .markdown pre,
        html.dark .swagger-ui .renderedMarkdown p,
        html.dark .swagger-ui .renderedMarkdown pre {
          color: #f1f5f9 !important; /* Lighter color with higher priority */
        }
        
        /* Ensure model property descriptions are visible */
        html.dark .swagger-ui .property-description {
          color: #e2e8f0 !important; /* Lighter color with higher priority */
        }
        
        /* Additional fixes for specific components */
        html.dark .swagger-ui .prop-type,
        html.dark .swagger-ui .prop-format {
          color: #94f2f5 !important; /* Bright cyan for property types */
        }
        
        html.dark .swagger-ui table.model tr.property-row td:first-of-type {
          color: #ffffff !important; /* White for property names */
        }
        
        html.dark .swagger-ui .parameter__extension,
        html.dark .swagger-ui .parameter__in {
          color: #bfdbfe !important; /* Light blue for parameters */
        }
        
        /* Better visibility for schemas */
        html.dark .swagger-ui .model-toggle:after {
          background-color: #94a3b8;
        }
        
        /* Improve visibility of required fields indicator */
        html.dark .swagger-ui .model td.col.model-schema .prop-required {
          color: #f87171 !important; /* Light red for required indicator */
        }
        
        /* Better example values */
        html.dark .swagger-ui .example {
          color: #c3ddfd !important;
        }
        
        /* Better required badge */
        html.dark .swagger-ui .parameter__name.required:after {
          color: #fca5a5 !important;
        }
        
        /* Make h4 headings more visible, especially for auth sections */
        html.dark .swagger-ui h4,
        html.dark .swagger-ui h4.opblock-title {
          color: #f8fafc !important; /* Very light gray, almost white */
          font-weight: 500;
        }
        
        html.dark .swagger-ui .auth-wrapper .auth-container h4 {
          color: #f8fafc !important;
          font-weight: 600;
        }
        
        html.dark .swagger-ui .auth-container .auth-btn-wrapper {
          color: #f8fafc;
        }
        
        html.dark .swagger-ui .auth-container .scopes h2 {
          color: #f8fafc !important;
        }
        
        /* Authentication method names (like cookieAuth) */
        html.dark .swagger-ui .auth-wrapper .authorization__title {
          color: #f8fafc !important;
        }
        
        /* Additional authentication method styling */
        html.dark .swagger-ui .auth-wrapper h4,
        html.dark .swagger-ui .auth-wrapper .authorization__title,
        html.dark .swagger-ui .auth-wrapper .auth-container h4,
        html.dark .swagger-ui .scheme-container .schemes-title {
          color: #ffffff !important; /* Pure white for auth titles */
          font-weight: 600;
          text-shadow: 0 0 1px rgba(255,255,255,0.3);
        }
        
        html.dark .swagger-ui .scheme-container .schemes-title {
          margin-bottom: 10px;
          font-size: 14px;
        }
        
        /* Cookie auth labels specifically */
        html.dark .swagger-ui .auth-container .wrapper label {
          color: #ffffff !important;
        }
        
        /* Target auth method names directly with high specificity */
        html.dark .swagger-ui .security-definitions-wrap section h4,
        html.dark .swagger-ui .security-definitions .security-definition-title,
        html.dark .swagger-ui .security-definitions h3 {
          color: #ffffff !important;
          font-weight: 600;
          text-shadow: 0 0 1px rgba(255,255,255,0.2);
          font-size: 16px;
        }
        
        /* Specific naming for cookieAuth */
        html.dark .swagger-ui .security-definitions .security-title {
          color: #ffffff !important;
          font-weight: 600;
        }
        
        /* Security definitions description */
        html.dark .swagger-ui .security-definitions .scopes {
          color: #e2e8f0 !important;
        }
        
        /* Improve visibility of subtitles and descriptions */
        html.dark .swagger-ui .opblock-description-wrapper h4,
        html.dark .swagger-ui .opblock-description-wrapper h5,
        html.dark .swagger-ui .opblock-description-wrapper p strong,
        html.dark .swagger-ui .opblock-section-header h4,
        html.dark .swagger-ui .opblock-section-header h4 span,
        html.dark .swagger-ui .opblock-title_normal,
        html.dark .swagger-ui .opblock-description-wrapper .renderedMarkdown p,
        html.dark .swagger-ui .opblock-external-docs-wrapper h4,
        html.dark .swagger-ui .auth-section-title,
        html.dark .swagger-ui .responses-header {
          color: #ffffff !important;
          font-weight: 600;
        }
        
        /* Specific targeting for "Authenticate user" and similar subtitles */
        html.dark .swagger-ui .opblock-description-wrapper h4,
        html.dark .swagger-ui .opblock-section-header h4 {
          color: #ffffff !important;
          font-weight: 600;
          font-size: 15px;
          margin-top: 10px;
          margin-bottom: 5px;
        }
        
        /* Make paragraph text in descriptions more visible */
        html.dark .swagger-ui .opblock-description p {
          color: #f8fafc !important;
        }
        
        /* Additional targeting for operation summaries and descriptions */
        html.dark .swagger-ui .opblock .opblock-summary-description {
          color: #f8fafc !important;
          font-weight: 500;
        }
        
        html.dark .swagger-ui table thead tr td,
        html.dark .swagger-ui table thead tr th {
          color: #f8fafc !important;
          font-weight: 600;
        }
        
        /* Make operation path summaries more visible */
        html.dark .swagger-ui .opblock-summary-path {
          color: #f8fafc !important;
          font-weight: 600;
        }
        
        /* Ensure strong tags in descriptions stand out */
        html.dark .swagger-ui .opblock-description-wrapper strong,
        html.dark .swagger-ui .opblock-summary strong,
        html.dark .swagger-ui .opblock-description strong,
        html.dark .swagger-ui .renderedMarkdown strong {
          color: #ffffff !important;
          font-weight: 600;
        }
        
        html.dark .swagger-ui .authorization__btn {
          color: #f97316;
        }
      `}</style>
    </div>
  );
} 