namespace QbKit.New;

public sealed class CounterComponentScaffolder
{
    public void Scaffold(string directory, string name)
    {
        var sourceRoot = Path.Combine(directory, "projects", name, "src");
        var appRoot = Path.Combine(sourceRoot, "app");
        var counterRoot = Path.Combine(appRoot, "counter");
        Directory.CreateDirectory(counterRoot);

        Write(Path.Combine(appRoot, "app.ts"), AppComponent);
        Write(Path.Combine(appRoot, "app.html"), AppTemplate);
        Write(Path.Combine(appRoot, "app.scss"), AppStyles);
        Write(Path.Combine(appRoot, "app.spec.ts"), AppSpec);

        Write(Path.Combine(counterRoot, "counter.ts"), CounterComponent);
        Write(Path.Combine(counterRoot, "counter.html"), CounterTemplate);
        Write(Path.Combine(counterRoot, "counter.scss"), CounterStyles);
        Write(Path.Combine(counterRoot, "counter.spec.ts"), CounterSpec);

        Write(Path.Combine(sourceRoot, "styles.scss"), GlobalStyles);
    }

    private static void Write(string path, string content) => File.WriteAllText(path, content + Environment.NewLine);

    private const string AppComponent = """
        import { Component } from '@angular/core';
        import { Counter } from './counter/counter';

        @Component({
          selector: 'app-root',
          imports: [Counter],
          templateUrl: './app.html',
          styleUrl: './app.scss',
        })
        export class App {}
        """;

    private const string AppTemplate = """
        <main class="app-shell">
          <app-counter />
        </main>
        """;

    private const string AppStyles = """
        .app-shell {
          display: flex;
          min-height: 100dvh;
          align-items: center;
          justify-content: center;
          padding: var(--qb-space-md);
        }
        """;

    private const string AppSpec = """
        import { TestBed } from '@angular/core/testing';
        import { App } from './app';

        describe('App', () => {
          beforeEach(async () => {
            await TestBed.configureTestingModule({ imports: [App] }).compileComponents();
          });

          it('given the root app, when created, then it exists', () => {
            const fixture = TestBed.createComponent(App);
            expect(fixture.componentInstance).toBeTruthy();
          });

          it('given the root app, when rendered, then it displays the counter component', async () => {
            const fixture = TestBed.createComponent(App);
            await fixture.whenStable();
            const compiled = fixture.nativeElement as HTMLElement;
            expect(compiled.querySelector('app-counter')).toBeTruthy();
          });
        });
        """;

    private const string CounterComponent = """
        import { ChangeDetectionStrategy, Component, signal } from '@angular/core';

        @Component({
          selector: 'app-counter',
          changeDetection: ChangeDetectionStrategy.OnPush,
          templateUrl: './counter.html',
          styleUrl: './counter.scss',
        })
        export class Counter {
          protected readonly count = signal(0);

          protected increment(): void {
            this.count.update((value) => value + 1);
          }

          protected decrement(): void {
            this.count.update((value) => value - 1);
          }
        }
        """;

    private const string CounterTemplate = """
        <section class="counter" aria-labelledby="counter-heading">
          <h1 id="counter-heading" class="counter__title">Counter</h1>
          <p class="counter__value" aria-live="polite">{{ count() }}</p>
          <div class="counter__controls">
            <button
              type="button"
              class="counter__button"
              (click)="decrement()"
              aria-label="Decrement count"
            >
              &minus;
            </button>
            <button
              type="button"
              class="counter__button"
              (click)="increment()"
              aria-label="Increment count"
            >
              +
            </button>
          </div>
        </section>
        """;

    private const string CounterStyles = """
        .counter {
          display: flex;
          flex-direction: column;
          align-items: center;
          gap: var(--qb-space-lg);
          width: min(100%, 24rem);
          padding: var(--qb-space-xl) var(--qb-space-lg);
          border: 1px solid var(--qb-color-border);
          border-radius: var(--qb-radius-md);
          text-align: center;

          &__title {
            margin: 0;
            font-size: var(--qb-font-size-display);
          }

          &__value {
            margin: 0;
            font-size: clamp(2.5rem, 2rem + 2vw, 4rem);
            font-variant-numeric: tabular-nums;
          }

          &__controls {
            display: flex;
            flex-wrap: wrap;
            justify-content: center;
            gap: var(--qb-space-md);
          }

          &__button {
            min-width: var(--qb-touch-target);
            min-height: var(--qb-touch-target);
            padding-inline: var(--qb-space-md);
            border: 1px solid var(--qb-color-border);
            border-radius: var(--qb-radius-md);
            background: var(--qb-color-primary);
            color: var(--qb-color-primary-contrast);
            font-size: 1.5rem;
            line-height: 1;
            cursor: pointer;
            transition:
              transform 0.15s ease,
              background-color 0.15s ease;

            &:hover {
              background-color: var(--qb-color-primary-hover);
            }

            &:focus-visible {
              outline: 2px solid var(--qb-color-primary);
              outline-offset: 2px;
            }

            &:active {
              transform: scale(0.96);
            }
          }
        }

        @media (prefers-reduced-motion: reduce) {
          .counter__button {
            transition: none;
          }
        }

        @media (max-width: 480px) {
          .counter {
            padding: var(--qb-space-lg) var(--qb-space-md);
          }
        }
        """;

    private const string CounterSpec = """
        import { TestBed } from '@angular/core/testing';
        import { Counter } from './counter';

        describe('Counter', () => {
          const createComponent = async () => {
            await TestBed.configureTestingModule({ imports: [Counter] }).compileComponents();
            const fixture = TestBed.createComponent(Counter);
            await fixture.whenStable();
            return fixture;
          };

          const valueText = (fixture: ReturnType<typeof TestBed.createComponent<Counter>>) =>
            (fixture.nativeElement as HTMLElement).querySelector('.counter__value')?.textContent?.trim();

          it('given a new counter, when rendered, then it displays zero', async () => {
            const fixture = await createComponent();
            expect(valueText(fixture)).toBe('0');
          });

          it('given a counter showing zero, when the increment button is clicked, then it displays one', async () => {
            const fixture = await createComponent();
            const incrementButton = (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>(
              'button[aria-label="Increment count"]',
            )!;
            incrementButton.click();
            await fixture.whenStable();
            expect(valueText(fixture)).toBe('1');
          });

          it('given a counter showing zero, when the decrement button is clicked, then it displays minus one', async () => {
            const fixture = await createComponent();
            const decrementButton = (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>(
              'button[aria-label="Decrement count"]',
            )!;
            decrementButton.click();
            await fixture.whenStable();
            expect(valueText(fixture)).toBe('-1');
          });

          it('given several clicks on both buttons, when rendered, then it reflects the net count', async () => {
            const fixture = await createComponent();
            const element = fixture.nativeElement as HTMLElement;
            const incrementButton = element.querySelector<HTMLButtonElement>(
              'button[aria-label="Increment count"]',
            )!;
            const decrementButton = element.querySelector<HTMLButtonElement>(
              'button[aria-label="Decrement count"]',
            )!;
            incrementButton.click();
            incrementButton.click();
            incrementButton.click();
            decrementButton.click();
            await fixture.whenStable();
            expect(valueText(fixture)).toBe('2');
          });
        });
        """;

    private const string GlobalStyles = """
        :root {
          color-scheme: light dark;

          --qb-color-surface: #ffffff;
          --qb-color-text: #1b1b1f;
          --qb-color-primary: #2563eb;
          --qb-color-primary-hover: #1d4ed8;
          --qb-color-primary-contrast: #ffffff;
          --qb-color-border: #d4d4d8;

          --qb-space-xs: 0.25rem;
          --qb-space-sm: 0.5rem;
          --qb-space-md: 1rem;
          --qb-space-lg: 1.5rem;
          --qb-space-xl: 2rem;

          --qb-font-size-base: clamp(1rem, 0.9rem + 0.3vw, 1.125rem);
          --qb-font-size-display: clamp(1.5rem, 1.3rem + 1vw, 2.25rem);

          --qb-radius-md: 0.5rem;
          --qb-touch-target: 2.75rem;
        }

        @media (prefers-color-scheme: dark) {
          :root {
            --qb-color-surface: #18181b;
            --qb-color-text: #f4f4f5;
            --qb-color-border: #3f3f46;
            --qb-color-primary-hover: #3b82f6;
          }
        }

        * {
          box-sizing: border-box;
        }

        html,
        body {
          height: 100%;
          margin: 0;
        }

        body {
          font-family:
            system-ui,
            -apple-system,
            'Segoe UI',
            Roboto,
            sans-serif;
          font-size: var(--qb-font-size-base);
          background: var(--qb-color-surface);
          color: var(--qb-color-text);
        }
        """;
}
