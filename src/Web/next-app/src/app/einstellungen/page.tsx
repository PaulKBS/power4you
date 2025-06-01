'use client';

import { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/contexts/AuthContext';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Form, FormControl, FormField, FormItem, FormLabel, FormMessage } from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { zodResolver } from "@hookform/resolvers/zod";
import { useForm, FieldErrors } from "react-hook-form";
import { z } from "zod";
import { Toaster } from "@/components/ui/sonner";
import { toast } from "sonner";
import { User, BookOpen, Key } from 'lucide-react';
import Link from 'next/link';

// Define the form schema
const formSchema = z.object({
  vorname: z.string().min(2, {
    message: "Vorname muss mindestens 2 Zeichen lang sein.",
  }),
  nachname: z.string().min(2, {
    message: "Nachname muss mindestens 2 Zeichen lang sein.",
  }),
  strasse: z.string().min(2, {
    message: "Straße muss mindestens 2 Zeichen lang sein.",
  }),
  hausnummer: z.string().min(1, {
    message: "Hausnummer ist erforderlich.",
  }),
  postleitzahl: z.string().min(5, {
    message: "Postleitzahl muss 5 Zeichen lang sein.",
  }).max(5, {
    message: "Postleitzahl muss 5 Zeichen lang sein.",
  }),
  ort: z.string().min(2, {
    message: "Ort muss mindestens 2 Zeichen lang sein.",
  }),
  email: z.string().email({
    message: "Bitte geben Sie eine gültige E-Mail-Adresse ein.",
  }),
  telefonnummer: z.string().min(6, {
    message: "Telefonnummer muss mindestens 6 Zeichen lang sein.",
  }),
});

export default function SettingsPage() {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const { user, loading } = useAuth();
  const router = useRouter();

  // Initialize the form
  const form = useForm<z.infer<typeof formSchema>>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      vorname: "",
      nachname: "",
      strasse: "",
      hausnummer: "",
      postleitzahl: "",
      ort: "",
      email: "",
      telefonnummer: "",
    },
    mode: "onSubmit",
  });
  
  // Fetch customer data when user is available
  useEffect(() => {
    if (!loading && !user) {
      router.push('/login');
      return;
    }

    async function fetchCustomerData() {
      if (!user) return;
      
      try {
        const response = await fetch('/api/customer');
        
        if (response.ok) {
          const data = await response.json();
          
          // Set form values with customer data
          form.reset({
            vorname: data.vorname,
            nachname: data.nachname,
            strasse: data.strasse,
            hausnummer: data.hausnummer,
            postleitzahl: data.postleitzahl,
            ort: data.ort,
            email: data.email,
            telefonnummer: data.telefonnummer,
          });
        }
      } catch (error) {
        console.error('Error fetching customer data:', error);
        toast.error("Fehler beim Laden der Kundendaten");
      }
    }
    
    if (user) {
      fetchCustomerData();
    }
  }, [user, loading, router, form]);

  // Handle form submission
  async function onSubmit(values: z.infer<typeof formSchema>) {
    if (isSubmitting) return;
    
    setIsSubmitting(true);
    
    try {
      const response = await fetch('/api/customer', {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(values),
      });
      
      const data = await response.json();
      
      if (response.ok) {
        toast.success("Ihre Daten wurden erfolgreich aktualisiert!");
        // Refresh the page data
        const refreshResponse = await fetch('/api/customer');
        if (refreshResponse.ok) {
          const refreshData = await refreshResponse.json();
          form.reset(refreshData);
        }
      } else {
        toast.error("Fehler beim Aktualisieren: " + data.message);
      }
    } catch (error) {
      toast.error("Es gab ein Problem beim Aktualisieren Ihrer Daten. Bitte versuchen Sie es später erneut.");
      console.error('Error updating customer data:', error);
    } finally {
      setIsSubmitting(false);
    }
  }

  // Handle form validation errors
  const onError = (errors: FieldErrors<z.infer<typeof formSchema>>) => {
    // Get all error messages
    const errorMessages = Object.entries(errors).map(([field, error]) => ({
      field,
      message: error?.message as string || "Ungültige Eingabe"
    }));

    // Show each error as a toast
    errorMessages.forEach(({ field, message }) => {
      const fieldName = field.charAt(0).toUpperCase() + field.slice(1);
      toast.error(`${fieldName}: ${message}`);
    });
  };

  if (loading) {
    return (
      <div className="p-6 max-w-7xl mx-auto">
        <div className="text-center py-12">
          <div className="inline-block h-8 w-8 animate-spin rounded-full border-4 border-solid border-orange-500 border-r-transparent"></div>
          <p className="mt-4 text-gray-600">Lade Einstellungen...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="p-6 max-w-7xl mx-auto">
      <Card>
        <CardHeader>
          <CardTitle>Einstellungen</CardTitle>
          <CardDescription>
            Verwalten Sie Ihre persönlichen Daten und Einstellungen
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit, onError)} className="space-y-6">
              <Tabs defaultValue="profile" className="w-full">
                <TabsList className="grid w-full grid-cols-2">
                  <TabsTrigger value="profile" className="flex items-center gap-2">
                    <User className="h-4 w-4" />
                    Profil
                  </TabsTrigger>
                  <TabsTrigger value="api" className="flex items-center gap-2">
                    <Key className="h-4 w-4" />
                    API
                  </TabsTrigger>
                </TabsList>

                <TabsContent value="profile" className="space-y-6 mt-6">
                  <div className="grid gap-6 md:grid-cols-2">
                    <FormField
                      control={form.control}
                      name="vorname"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Vorname</FormLabel>
                          <FormControl>
                            <Input placeholder="Ihr Vorname" {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                    <FormField
                      control={form.control}
                      name="nachname"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Nachname</FormLabel>
                          <FormControl>
                            <Input placeholder="Ihr Nachname" {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                  </div>
                  
                  <div className="grid gap-6 md:grid-cols-2">
                    <FormField
                      control={form.control}
                      name="email"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>E-Mail</FormLabel>
                          <FormControl>
                            <Input placeholder="Ihre E-Mail-Adresse" type="email" {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                    <FormField
                      control={form.control}
                      name="telefonnummer"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Telefonnummer</FormLabel>
                          <FormControl>
                            <Input placeholder="Ihre Telefonnummer" type="tel" {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                  </div>
                  
                  <div className="grid gap-6 md:grid-cols-2">
                    <FormField
                      control={form.control}
                      name="strasse"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Straße</FormLabel>
                          <FormControl>
                            <Input placeholder="Ihre Straße" {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                    <FormField
                      control={form.control}
                      name="hausnummer"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Hausnummer</FormLabel>
                          <FormControl>
                            <Input placeholder="Ihre Hausnummer" {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                  </div>
                  
                  <div className="grid gap-6 md:grid-cols-2">
                    <FormField
                      control={form.control}
                      name="postleitzahl"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Postleitzahl</FormLabel>
                          <FormControl>
                            <Input placeholder="Ihre Postleitzahl" {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                    <FormField
                      control={form.control}
                      name="ort"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Ort</FormLabel>
                          <FormControl>
                            <Input placeholder="Ihr Ort" {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                  </div>
                </TabsContent>

                <TabsContent value="api" className="space-y-6 mt-6">
                  <div className="space-y-4">
                    <div>
                      <h3 className="text-lg font-medium">API-Schlüssel</h3>
                      <p className="text-sm text-gray-500 mb-4">
                        Ihr API-Schlüssel für den Zugriff auf die Power4You-API.
                      </p>
                      <div className="flex items-center gap-4">
                        <Input 
                          value={user?.apiKey || ''} 
                          readOnly 
                          className="font-mono bg-gray-50"
                        />
                        <Button
                          type="button"
                          variant="outline"
                          onClick={() => {
                            if (user?.apiKey) {
                              navigator.clipboard.writeText(user.apiKey);
                              toast.success("API-Schlüssel in die Zwischenablage kopiert!");
                            }
                          }}
                        >
                          Kopieren
                        </Button>
                      </div>
                      <p className="text-sm text-amber-600 mt-2">
                        <strong>Wichtig:</strong> Halten Sie Ihren API-Schlüssel geheim. Er gewährt vollen Zugriff auf Ihr Konto.
                      </p>
                    </div>
                    
                    <div className="pt-4 border-t border-gray-200">
                      <h3 className="text-lg font-medium flex items-center gap-2">
                        <BookOpen className="h-5 w-5" />
                        API-Dokumentation
                      </h3>
                      <p className="text-sm text-gray-500 mb-3">
                        Entdecken Sie unsere umfassende API-Dokumentation für Entwickler.
                      </p>
                      <Link href="/api-doc">
                        <Button variant="secondary" className="flex items-center gap-2">
                          <BookOpen className="h-4 w-4" />
                          Zur API-Dokumentation
                        </Button>
                      </Link>
                    </div>
                  </div>
                </TabsContent>
              </Tabs>

              <div className="flex justify-end pt-6">
                <Button type="submit" disabled={isSubmitting}>
                  {isSubmitting ? "Wird gespeichert..." : "Speichern"}
                </Button>
              </div>
            </form>
          </Form>
        </CardContent>
      </Card>
      <Toaster />
    </div>
  );
} 