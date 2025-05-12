'use client';

import { useState } from 'react';
import { useAuth } from '@/contexts/AuthContext';
import { Button } from '@/components/ui/button';
import Link from 'next/link';
import { Menu, X, Home, HelpCircle, Sun, Settings } from 'lucide-react';
import { cn } from '@/lib/utils';
import { usePathname } from 'next/navigation';
import { ThemeToggle } from '@/components/ui/theme-toggle';

export default function Navbar() {
  const { user, logout } = useAuth();
  const pathname = usePathname();
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

  const toggleMobileMenu = () => {
    setMobileMenuOpen(!mobileMenuOpen);
  };

  return (
    <nav className="sticky top-0 z-50 w-full border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <div className="container mx-auto flex h-16 items-center justify-between px-4">
        <div className="flex items-center gap-3 md:gap-8">
          <Link href="/" className="flex items-center gap-2">
            <span className="hidden text-xl font-bold text-foreground md:inline-block">
              power4you <span className="text-orange-500">🔥</span>
            </span>
            <span className="text-xl font-bold text-foreground md:hidden">
              p4y <span className="text-orange-500">🔥</span>
            </span>
          </Link>
          <div className="hidden md:flex md:gap-8">
            <NavLink href="/" active={pathname === '/'}>
              <Home className="h-4 w-4" />
              <span>Home</span>
            </NavLink>
            <NavLink href="/anlagen" active={pathname === '/anlagen' || pathname.startsWith('/anlagen/')}>
              <Sun className="h-4 w-4" />
              <span>Anlagen</span>
            </NavLink>
            <NavLink href="/support" active={pathname === '/support'}>
              <HelpCircle className="h-4 w-4" />
              <span>Support</span>
            </NavLink>
            <NavLink href="/einstellungen" active={pathname === '/einstellungen'}>
              <Settings className="h-4 w-4" />
              <span>Einstellungen</span>
            </NavLink>
          </div>
        </div>
        
        <div className="flex items-center space-x-4">
          <ThemeToggle />
          {user ? (
            <>
              <span className="hidden text-sm text-muted-foreground md:inline-block">
                Willkommen {user.kunde ? `${user.kunde.vorname} ${user.kunde.nachname}!` : user.username}
              </span>
              <Button 
                variant="outline" 
                onClick={logout} 
                size="sm"
                className="h-8"
              >
                Logout
              </Button>
            </>
          ) : (
            <Button 
              variant="default" 
              size="sm" 
              asChild
              className="h-8"
            >
              <Link href="/login">Login</Link>
            </Button>
          )}
          <Button
            variant="ghost"
            size="icon"
            className="md:hidden"
            aria-label="Toggle menu"
            onClick={toggleMobileMenu}
          >
            {mobileMenuOpen ? (
              <X className="h-5 w-5" />
            ) : (
              <Menu className="h-5 w-5" />
            )}
          </Button>
        </div>
      </div>

      {/* Mobile menu */}
      {mobileMenuOpen && (
        <div className="md:hidden bg-background border-b">
          <div className="container mx-auto py-4 px-4 space-y-3">
            <MobileNavLink href="/" active={pathname === '/'} onClick={() => setMobileMenuOpen(false)}>
              <Home className="h-4 w-4" />
              <span>Home</span>
            </MobileNavLink>
            <MobileNavLink 
              href="/anlagen" 
              active={pathname === '/anlagen' || pathname.startsWith('/anlagen/')}
              onClick={() => setMobileMenuOpen(false)}
            >
              <Sun className="h-4 w-4" />
              <span>Anlagen</span>
            </MobileNavLink>
            <MobileNavLink 
              href="/support" 
              active={pathname === '/support'}
              onClick={() => setMobileMenuOpen(false)}
            >
              <HelpCircle className="h-4 w-4" />
              <span>Support</span>
            </MobileNavLink>
            <MobileNavLink 
              href="/einstellungen" 
              active={pathname === '/einstellungen'}
              onClick={() => setMobileMenuOpen(false)}
            >
              <Settings className="h-4 w-4" />
              <span>Einstellungen</span>
            </MobileNavLink>
          </div>
        </div>
      )}
    </nav>
  );
}

interface NavLinkProps {
  href: string;
  children: React.ReactNode;
  active?: boolean;
}

function NavLink({ href, children, active }: NavLinkProps) {
  return (
    <Link 
      href={href}
      className={cn(
        "group flex items-center gap-1.5 text-sm font-medium transition-colors",
        active 
          ? "text-foreground" 
          : "text-muted-foreground hover:text-foreground"
      )}
    >
      {children}
    </Link>
  );
}

interface MobileNavLinkProps extends NavLinkProps {
  onClick?: () => void;
}

function MobileNavLink({ href, children, active, onClick }: MobileNavLinkProps) {
  return (
    <Link 
      href={href}
      className={cn(
        "flex items-center gap-2 rounded-md p-2 text-sm font-medium transition-colors",
        active 
          ? "bg-muted text-foreground" 
          : "text-muted-foreground hover:bg-muted hover:text-foreground"
      )}
      onClick={onClick}
    >
      {children}
    </Link>
  );
} 