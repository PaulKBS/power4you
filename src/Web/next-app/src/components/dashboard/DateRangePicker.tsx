'use client';

import { useState } from 'react';
import { format } from "date-fns";
import { Calendar as CalendarIcon } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Calendar } from "@/components/ui/calendar";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";

import { cn } from "@/lib/utils";

interface DateRangePickerProps {
  onDateRangeChange: (startDate: string, endDate: string, view: 'hourly' | 'daily') => void;
  isLoading?: boolean;
}

export default function DateRangePicker({ onDateRangeChange, isLoading }: DateRangePickerProps) {
  const [startDate, setStartDate] = useState<Date>();
  const [endDate, setEndDate] = useState<Date>();
  const [view, setView] = useState<'hourly' | 'daily'>('hourly');
  const [startOpen, setStartOpen] = useState(false);
  const [endOpen, setEndOpen] = useState(false);

  // Get date for quick ranges
  const getDateFromDaysAgo = (daysAgo: number) => {
    const date = new Date();
    date.setDate(date.getDate() - daysAgo);
    return date;
  };

  // Quick date range buttons
  const quickRanges = [
    { label: 'Heute', days: 0, view: 'hourly' as const },
    { label: 'Gestern', days: 1, view: 'hourly' as const },
    { label: '7 Tage', days: 7, view: 'daily' as const },
    { label: '30 Tage', days: 30, view: 'daily' as const },
  ];

  const handleQuickRange = (days: number, viewType: 'hourly' | 'daily') => {
    const end = new Date();
    const start = days === 0 ? new Date() : getDateFromDaysAgo(days);
    setStartDate(start);
    setEndDate(end);
    setView(viewType);
    onDateRangeChange(
      start.toISOString().split('T')[0], 
      end.toISOString().split('T')[0], 
      viewType
    );
  };

  const handleCustomRange = () => {
    if (startDate && endDate) {
      onDateRangeChange(
        startDate.toISOString().split('T')[0],
        endDate.toISOString().split('T')[0],
        view
      );
    }
  };

  const handleStartDateSelect = (date: Date | undefined) => {
    setStartDate(date);
    setStartOpen(false);
    if (date && endDate && date <= endDate) {
      handleCustomRange();
    }
  };

  const handleEndDateSelect = (date: Date | undefined) => {
    setEndDate(date);
    setEndOpen(false);
    if (startDate && date && startDate <= date) {
      handleCustomRange();
    }
  };

  return (
    <div className="bg-card border rounded-lg p-4 mb-6">
      <div className="flex flex-col md:flex-row md:items-center gap-4">
        {/* Quick range buttons */}
        <div className="flex gap-2">
          <span className="text-sm font-medium text-muted-foreground flex items-center mr-2">
            Zeitraum:
          </span>
          {quickRanges.map((range, index) => (
            <Button
              key={index}
              variant="outline"
              size="sm"
              onClick={() => handleQuickRange(range.days, range.view)}
              disabled={isLoading}
              className="text-xs h-8"
            >
              {range.label}
            </Button>
          ))}
        </div>

        {/* Divider */}
        <div className="hidden md:block w-px h-6 bg-border"></div>

        {/* Custom date range */}
        <div className="flex items-center gap-3">
          <span className="text-sm text-muted-foreground">Von:</span>
          <Popover open={startOpen} onOpenChange={setStartOpen}>
            <PopoverTrigger asChild>
              <Button
                variant="outline"
                className={cn(
                  "w-[140px] justify-start text-left font-normal h-8 text-xs",
                  !startDate && "text-muted-foreground"
                )}
                disabled={isLoading}
              >
                <CalendarIcon className="mr-2 h-3 w-3" />
                {startDate ? format(startDate, "dd.MM.yyyy") : <span>Datum wählen</span>}
              </Button>
            </PopoverTrigger>
            <PopoverContent className="w-auto p-0">
              <Calendar
                mode="single"
                selected={startDate}
                onSelect={handleStartDateSelect}
                disabled={(date) => {
                  const today = new Date();
                  return date > today || (endDate ? date > endDate : false);
                }}
                initialFocus
              />
            </PopoverContent>
          </Popover>

          <span className="text-sm text-muted-foreground">Bis:</span>
          <Popover open={endOpen} onOpenChange={setEndOpen}>
            <PopoverTrigger asChild>
              <Button
                variant="outline"
                className={cn(
                  "w-[140px] justify-start text-left font-normal h-8 text-xs",
                  !endDate && "text-muted-foreground"
                )}
                disabled={isLoading}
              >
                <CalendarIcon className="mr-2 h-3 w-3" />
                {endDate ? format(endDate, "dd.MM.yyyy") : <span>Datum wählen</span>}
              </Button>
            </PopoverTrigger>
            <PopoverContent className="w-auto p-0">
              <Calendar
                mode="single"
                selected={endDate}
                onSelect={handleEndDateSelect}
                disabled={(date) => {
                  const today = new Date();
                  return date > today || (startDate ? date < startDate : false);
                }}
                initialFocus
              />
            </PopoverContent>
          </Popover>



          <Button
            onClick={handleCustomRange}
            disabled={!startDate || !endDate || isLoading}
            size="sm"
            className="h-8"
          >
            {isLoading ? 'Lädt...' : 'Anwenden'}
          </Button>
        </div>
      </div>

      {/* Current selection display */}
      {startDate && endDate && (
        <div className="mt-3 text-xs text-muted-foreground">
          Aktuell: {format(startDate, "dd.MM.yyyy")} - {format(endDate, "dd.MM.yyyy")} 
          ({view === 'hourly' ? 'Stündlich' : 'Täglich'})
        </div>
      )}
    </div>
  );
} 