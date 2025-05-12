'use client';

import { useState, useEffect } from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { Toaster } from "@/components/ui/sonner";
import { toast } from "sonner";
import { useAuth } from "@/contexts/AuthContext";

// Define the form schema
const formSchema = z.object({
  name: z.string().min(2, {
    message: "Name muss mindestens 2 Zeichen lang sein.",
  }),
  email: z.string().email({
    message: "Bitte geben Sie eine gültige E-Mail-Adresse ein.",
  }),
  subject: z.string().min(5, {
    message: "Betreff muss mindestens 5 Zeichen lang sein.",
  }),
  message: z.string().min(10, {
    message: "Nachricht muss mindestens 10 Zeichen lang sein.",
  }),
});

export default function SupportPage() {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [customerData, setCustomerData] = useState<{ vorname: string; nachname: string; email: string } | null>(null);
  const { user } = useAuth();

  // Initialize the form
  const form = useForm<z.infer<typeof formSchema>>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      name: "",
      email: "",
      subject: "",
      message: "",
    },
  });
  
  // Fetch customer data when user is available
  useEffect(() => {
    async function fetchCustomerData() {
      if (!user) return;
      
      try {
        const response = await fetch('/api/customer');
        
        if (response.ok) {
          const data = await response.json();
          setCustomerData(data);
          
          // Set form values with customer data
          const fullName = `${data.vorname} ${data.nachname}`;
          form.setValue('name', fullName);
          form.setValue('email', data.email);
        }
      } catch (error) {
        console.error('Error fetching customer data:', error);
      }
    }
    
    fetchCustomerData();
  }, [user, form]);

  // Handle form submission
  async function onSubmit(values: z.infer<typeof formSchema>) {
    setIsSubmitting(true);
    
    try {
      const response = await fetch('/api/contact', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(values),
      });
      
      const data = await response.json();
      
      if (response.ok) {
        toast.success("Ihre Nachricht wurde erfolgreich gesendet!");
        form.reset();
      } else {
        toast.error("Fehler beim Senden: " + data.message);
      }
    } catch (error) {
      toast.error("Es gab ein Problem beim Senden Ihrer Nachricht. Bitte versuchen Sie es später erneut.");
      console.error('Error sending message:', error);
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <div className="container mx-auto py-12 max-w-3xl">
      <Toaster />
      <Card>
        <CardHeader className="text-center">
          <CardTitle className="text-3xl mb-2">Kontakt & Support</CardTitle>
          <CardDescription>
            Haben Sie Fragen oder benötigen Sie Hilfe? Kontaktieren Sie uns über das Formular unten.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <FormField
                  control={form.control}
                  name="name"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Name</FormLabel>
                      <FormControl>
                        <Input 
                          placeholder="Ihr Name" 
                          {...field} 
                          disabled={customerData !== null}
                          className={customerData ? "text-muted-foreground bg-muted" : ""}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="email"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>E-Mail</FormLabel>
                      <FormControl>
                        <Input 
                          placeholder="Ihre E-Mail-Adresse" 
                          type="email" 
                          {...field} 
                          disabled={customerData !== null}
                          className={customerData ? "text-muted-foreground bg-muted" : ""}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>
              <FormField
                control={form.control}
                name="subject"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Betreff</FormLabel>
                    <FormControl>
                      <Input placeholder="Betreff Ihrer Anfrage" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="message"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Nachricht</FormLabel>
                    <FormControl>
                      <Textarea 
                        placeholder="Ihre Nachricht..." 
                        className="min-h-[150px]" 
                        {...field} 
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <Button 
                type="submit" 
                className="w-full"
                disabled={isSubmitting}
              >
                {isSubmitting ? "Senden..." : "Nachricht senden"}
              </Button>
            </form>
          </Form>
        </CardContent>
      </Card>
    </div>
  );
} 