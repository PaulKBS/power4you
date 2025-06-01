'use client';

import React from 'react';

export function SolarPanelAnimation() {
  return (
    <div className="relative w-full h-full flex items-center justify-center">
      {/* Solar panel frame */}
      <div className="relative w-32 h-24 bg-slate-800 rounded-md overflow-hidden flex flex-col p-1">
        {/* Solar cells grid */}
        <div className="grid grid-cols-3 grid-rows-3 gap-[2px] flex-1">
          {[...Array(9)].map((_, i) => (
            <div 
              key={i} 
              className="bg-blue-900 rounded-sm relative overflow-hidden"
              style={{ 
                animation: `solarGlint ${3 + (i % 3)}s ease-in-out infinite ${i * 0.2}s` 
              }}
            >
              <div className="absolute inset-0 bg-gradient-to-br from-blue-400 to-transparent opacity-0" 
                style={{ animation: `solarShine ${4 + (i % 3)}s ease-in-out infinite ${i * 0.3}s` }} 
              />
            </div>
          ))}
        </div>
        
        {/* Mount stand */}
        <div className="absolute -bottom-1 left-1/2 transform -translate-x-1/2 w-10 h-6">
          <div className="w-1.5 h-6 bg-slate-700 mx-auto" />
          <div className="w-8 h-1.5 bg-slate-800 absolute bottom-0 left-1/2 transform -translate-x-1/2 rounded-sm" />
        </div>
      </div>
      
      {/* Sun rays animation */}
      <div className="absolute w-full h-full pointer-events-none">
        <div className="absolute top-2 right-2 w-8 h-8 rounded-full bg-sky-300 opacity-80 animate-pulse" />
        <div 
          className="absolute top-0 left-0 w-full h-full opacity-30"
          style={{ 
            background: 'radial-gradient(circle at top right, rgba(186, 230, 253, 0.9), transparent 70%)',
            animation: 'solarRays 8s ease-in-out infinite'
          }}
        />
        
        {/* Energy particles */}
        {[...Array(5)].map((_, i) => (
          <div 
            key={i}
            className="absolute w-1 h-1 bg-cyan-300 rounded-full opacity-80"
            style={{
              top: `${20 + Math.random() * 10}%`,
              right: `${30 + Math.random() * 20}%`,
              animation: `energyParticle ${3 + Math.random() * 2}s ease-in-out infinite ${i * 0.5}s`
            }}
          />
        ))}
      </div>

      <style jsx global>{`
        @keyframes solarGlint {
          0%, 100% { transform: scale(1); filter: brightness(0.9); }
          50% { transform: scale(1.02); filter: brightness(1.2); }
        }
        
        @keyframes solarShine {
          0%, 100% { opacity: 0; transform: translateY(100%) translateX(100%); }
          50% { opacity: 0.4; transform: translateY(0%) translateX(0%); }
        }
        
        @keyframes solarRays {
          0%, 100% { opacity: 0.3; transform: scale(1) rotate(0deg); }
          50% { opacity: 0.4; transform: scale(1.1) rotate(5deg); }
        }
        
        @keyframes energyParticle {
          0% { opacity: 0; transform: translateY(0) scale(0); }
          50% { opacity: 0.8; transform: translateY(-20px) scale(1); }
          100% { opacity: 0; transform: translateY(-40px) scale(0); }
        }
      `}</style>
    </div>
  );
} 